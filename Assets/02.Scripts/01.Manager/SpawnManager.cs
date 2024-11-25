using Pathfinding;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class SpawnManager : MonoBehaviour, IUpdateable
{
    public static SpawnManager Instance { get; private set; }

    [BoxGroup("점수 세팅"), LabelText("점수별 단계"), SerializeField]
    private int[] stepByScore;

    [BoxGroup("위치 세팅"), LabelText("스폰 반경"), SerializeField]
    private float spawnRadius = 50f; // 생성 반경 (맵 밖에서 얼마나 떨어진 곳에 적을 생성할지 결정)
    [BoxGroup("위치 세팅"), LabelText("스폰 높이"), SerializeField]
    private float spawnHeight = 3f; // 적이 생성될 때의 높이

    [BoxGroup("플레이어 세팅"), LabelText("스폰 위치"), SerializeField]
    private Transform spawnTransfrom;


    [BoxGroup("적 세팅"), LabelText("현재 레벨"), SerializeField]
    private int step = 0;
    [BoxGroup("적 세팅"), LabelText("스폰하는 적 리스트"), SerializeField]
    private SpawnObjectsList[] spawnObjectsList;

    [BoxGroup("시민 세팅"), LabelText("스폰할 위치"), SerializeField]
    private Transform[] citizenSpawnTransforms;

    [BoxGroup("시민 세팅"), LabelText("스폰 범위"), SerializeField]
    private Vector3[] citizenSpawnRange;

    [BoxGroup("시민 세팅"), LabelText("게임에 시민 수"), SerializeField]
    private int[] maxCitizen;

    [BoxGroup("골드 세팅"), LabelText("스폰할 골드 수"), SerializeField]
    private int maxGoldCount = 10; // 스폰할 골드 수

    [FoldoutGroup("특수 오브젝트"), LabelText("폭격기"), SerializeField]
    private GameObject bomberPrefab;
    [FoldoutGroup("특수 오브젝트"), LabelText("모든 시민"), SerializeField]
    private GameObject[] allCitizen;
    [FoldoutGroup("특수 오브젝트"), LabelText("돈"), SerializeField]
    private GameObject goldPrefab;
    [FoldoutGroup("특수 오브젝트"), LabelText("부모 오브젝트"), SerializeField]
    private Transform[] parentTransforms;

    private List<List<GameObject>> currentCitizenList = new List<List<GameObject>>(); // 현재 게임 내 시민 리스트
    private List<GameObject> currentStepEnemies = new List<GameObject>();

    private Queue<GameObject> goldPool = new Queue<GameObject>(); // 골드 오브젝트 풀
    private List<GameObject> activeGoldList = new List<GameObject>(); // 활성화된 골드 리스트

    private bool isSceneClosing = false; // 씬이 닫히는 중인지 여부를 확인하는 플래그

    private Transform slimeTrans;
    private float bomberSpawnTimer = 0f;

    [SerializeField, LabelText("스폰 가능한 레이어")]
    private LayerMask surfaceLayerMask;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (stepByScore.Length != spawnObjectsList.Length)
        {
            Debug.LogError("점수별 단계와 단계별 적 리스트의 길이가 다릅니다.");
        }

        InitializeCitizenLists();

        SpawnPlayer();

        for (int i = 0; i < citizenSpawnTransforms.Length; i++)
        {
            SpawnCitizens(i); // 시민 스폰 초기화
        }

        //SpawnEnemiesForCurrentStep();

        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    // 씬이 닫힐 때 스폰을 중단하기 위해 플래그 설정
    private void OnApplicationQuit()
    {
        isSceneClosing = true; // 오브젝트가 파괴될 때도 플래그 설정
        CleanUpResources();
    }

    private void OnDestroy()
    {
        isSceneClosing = true; // 오브젝트가 파괴될 때도 플래그 설정

        // GameLogicManager가 null이 아니고, 인스턴스가 존재할 때만 Deregister 호출
        if (GameLogicManager.Instance != null)
        {
            GameLogicManager.Instance.DeregisterUpdatableObject(this);
        }

        CleanUpResources();

        AstarPath.active?.PausePathfinding();  // 경로 탐색 중지
        AstarPath.active?.FlushGraphUpdates();  // 그래프 업데이트 비우기
        Resources.UnloadUnusedAssets();  // 불필요한 리소스 정리
        System.GC.Collect();  // 가비지 컬렉션 강제 실행
    }

    public virtual void OnUpdate(float dt)
    {
        if(slimeTrans == null)
        {
            return;
        }
        
        if(GameManager.Instance.GetGameState())
        {
            // 스폰매니저의 위치를 플레이어(슬라임) 위치로 설정 (나중에 삭제)
            transform.position = new Vector3(slimeTrans.position.x, 0, slimeTrans.position.z);
        }

        //폭격기 스폰
        SpawnBomber();

        // 점수 체크
        CheckScore();

        for (int i = 0; i < citizenSpawnTransforms.Length; i++)
        {
            // 시민 수 관리
            ManageCitizenSpawn(i);
        }

        if (activeGoldList.Count < maxGoldCount)
        {
            SpawnGold();
        }
    }

    private void CleanUpResources()
    {
        for (int i = 0; i < citizenSpawnTransforms.Length; i++)
        {
            RemoveAllCitizens(i);
        }
        RemoveAllEnemy();
        RemoveAllGold();
    }

    private void InitializeCitizenLists()
    {
        // citizenSpawnTransforms 배열의 길이를 기준으로 리스트 개수를 맞춤
        int citizenCount = citizenSpawnTransforms.Length;

        currentCitizenList = new List<List<GameObject>>(citizenCount); // 리스트 크기 설정

        for (int i = 0; i < citizenCount; i++)
        {
            // 각 내부 리스트를 초기화
            currentCitizenList.Add(new List<GameObject>());
        }
    }

    public void StartSpawn()
    {
        SpawnEnemiesForCurrentStep();
    }

    // 점수를 체크하는 함수
    public void CheckScore()
    {
        if (step >= stepByScore.Length - 1)
        {
            return; // No more steps available
        }

        if (stepByScore[step] < GameManager.Instance.GetScore())
        {
            step++;
            SpawnEnemiesForCurrentStep();
        }
    }

    // 스텝 변경 시 호출되는 함수
    [Button]
    public void ChangeStep(int newStep)
    {
        // 새로운 스텝으로 변경
        step = newStep;

        // 새로운 스텝의 적들을 스폰
        SpawnEnemiesForCurrentStep();
    }

    // position에 충돌하는 것이 있는지 확인
    private bool IsPositionOccupied(Vector3 position, float radius)
    {
        // 주어진 위치에 일정 반경 내에 충돌체가 있는지 확인
        Collider[] colliders = Physics.OverlapSphere(position, radius);
        return colliders.Length > 0; // 만약 충돌체가 하나라도 있으면 위치가 점유된 것으로 판단
    }

    // 랜덤한 위치가 없을 때 다시 찾는 함수
    private Vector3 GetValidSpawnPosition()
    {
        Vector3 spawnPosition = Vector3.zero;
        int maxAttempts = 10;
        int attempts = 0;
        //float spawnCheckRadius = 2f; // 충돌 체크할 반경 (적 크기에 맞게 설정)
        //float maxNavMeshDistance = 10f; // 네비매쉬 표면과의 최대 거리 (네비매쉬 위치 샘플링 시 사용)

        while (attempts < maxAttempts)
        {
            // 랜덤한 위치를 생성
            Vector3 randomPosition = GetRandomSpawnPosition();

            // Use A* Pathfinding's GetNearest to find the closest graph node
            NNInfo nearestNodeInfo = AstarPath.active.GetNearest(randomPosition);
            if (nearestNodeInfo.node != null && nearestNodeInfo.node.Walkable)
            {
                spawnPosition = nearestNodeInfo.position;
                spawnPosition.y = spawnHeight; // Adjust height
                break;
            }

            attempts++;
        }
 
        return spawnPosition;
    }

    // 랜덤한 위치
    private Vector3 GetRandomSpawnPosition()
    {
        // XZ 평면에서 랜덤한 방향을 선택하여 생성 범위 외부의 랜덤 위치를 계산
        Vector2 randomDirection = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = new Vector3(randomDirection.x, spawnHeight, randomDirection.y);

        if (slimeTrans != null)
        {
            // 플레이어 또는 맵 중앙에서 먼 위치에 스폰되도록 설정
            spawnPosition += slimeTrans.position;
        }

        spawnPosition.y = spawnHeight;

        return spawnPosition;    
    }

    #region 플레이어
    private void SpawnPlayer()
    {
        //int index = Random.Range(0, spawnTransfrom);
        GameObject player = Instantiate(SkinManager.Instance.GetPlayer(), spawnTransfrom.position, Quaternion.identity);
        slimeTrans = player.transform;


        GetComponent<ProceduralGridMover>().target = player.transform;
    }

    public Transform GetPlayerTrans()
    {
        return slimeTrans;
    }
    #endregion

    #region 적
    // 현재 스텝의 적들을 스폰하는 함수
    private void SpawnEnemiesForCurrentStep()
    {
        // 현재 스텝에 있는 적을 스폰
        foreach (GameObject enemyPrefab in spawnObjectsList[step].SpawnObjects)
        {
            SpawnEnemy(enemyPrefab);
        }
    }

    // 적을 스폰하는 함수
    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if(slimeTrans == null)
        {
            return;
        }
        Vector3 spawnPosition = GetValidSpawnPosition();

        // 유효한 NavMesh 위의 위치를 찾았을 때만 적을 스폰
        if (spawnPosition == Vector3.zero) return;
        
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        newEnemy.transform.SetParent(parentTransforms[0]);
        // 스폰된 적을 현재 스텝 리스트에 추가
        currentStepEnemies.Add(newEnemy);

        // 적이 파괴되었을 때 다시 스폰하지 않도록 이전 스텝의 적인지 확인
        NPCBase enemyComponent = newEnemy.GetComponent<NPCBase>();
        if (enemyComponent != null)
        {
            enemyComponent.OnDestroyed += () =>
            {
                //RemoveEnemyFromList(newEnemy);
                if (!isSceneClosing && Application.isPlaying)
                {
                    // 적이 파괴된 이후에도 스폰매니저가 파괴되지 않았는지 확인
                    SpawnEnemy(enemyPrefab); // 다시 스폰 (현재 스텝의 적만)

                    // 적이 파괴되었을 때 리스트에서 제거
                    //RemoveEnemyFromList(newEnemy);
                }
                enemyComponent.OnDestroyed -= null; // Remove listener to avoid leaks
            };

            enemyComponent.SetTarget(slimeTrans);
        }
        
    }

    // 폭격기 스폰 함수
    private void SpawnBomber()
    {
        bomberSpawnTimer += Time.deltaTime;

        if ( spawnObjectsList[step].isBomber && 
            bomberSpawnTimer >= spawnObjectsList[step].spawnInterval )
        {
            bomberSpawnTimer = 0f;

            Vector3 spawnPosition = GetValidSpawnPosition();

            // 유효한 NavMesh 위의 위치를 찾았을 때만 적을 스폰
            if (spawnPosition != Vector3.zero)
            {
                GameObject newEnemy = Instantiate(bomberPrefab, spawnPosition, Quaternion.identity);
                
                // 적이 파괴되었을 때 다시 스폰하지 않도록 이전 스텝의 적인지 확인
                NPCBase enemyComponent = newEnemy.GetComponent<NPCBase>();
                enemyComponent.SetTarget(slimeTrans);
            }
        }
    }

    private void RemoveEnemyFromList(GameObject enemy)
    {
        currentStepEnemies.Remove(enemy);
        //beforeStepEnemies.Remove(enemy);
    }

    // 모든 적을 제거하는 함수
    public void RemoveAllEnemy()
    {
        foreach(GameObject enemy in currentStepEnemies)
        {
            Destroy(enemy);
        }
        currentStepEnemies.Clear();
    
        //foreach(GameObject enemy in beforeStepEnemies)
        //{
        //    Destroy(enemy);
        //}
        //beforeStepEnemies.Clear();
    }
    #endregion

    #region 시민
    public Vector3 GetRandomCitizenSpawnPoint(int i)
    {
        // 스폰 위치를 범위 내 랜덤하게 결정
        Vector3 randomPosition = new Vector3(
            Random.Range(-citizenSpawnRange[i].x / 2, citizenSpawnRange[i].x / 2),
            spawnHeight,
            Random.Range(-citizenSpawnRange[i].z / 2, citizenSpawnRange[i].z / 2)
        );

        // 스폰 위치는 스크립트가 위치한 오브젝트의 위치를 기준으로 함
        Vector3 spawnPosition = citizenSpawnTransforms[i].position + randomPosition;

        return spawnPosition;
    }

    private Vector3 GetValidSpawnPosition(int i)
    {
        Vector3 spawnPosition = Vector3.zero;
        int maxAttempts = 10; // 유효한 위치를 찾기 위한 최대 시도 횟수
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            // 랜덤한 위치를 생성
            spawnPosition = GetRandomCitizenSpawnPoint(i);

            // 아래로 Raycast를 사용해 지정된 레이어(suface)에 닿는지 확인
            if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, surfaceLayerMask))
            {
                spawnPosition = hit.point; // 충돌한 지점으로 위치를 설정
                spawnPosition.y += spawnHeight; // 스폰 높이를 적용
                break;
            }

            attempts++;
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning($"유효한 스폰 위치를 찾을 수 없습니다. 스폰 시도 횟수 초과 (Index: {i})");
            return Vector3.zero; // 실패한 경우 (0, 0, 0) 반환
        }

        return spawnPosition;
    }

    private void SpawnCitizens(int i)
    {
        if (slimeTrans == null)
            return;

        while (currentCitizenList[i].Count < maxCitizen[i])
        {
            GameObject citizenPrefab = allCitizen[Random.Range(0, allCitizen.Length)];
            Vector3 spawnPosition = GetValidSpawnPosition(i);

            if (spawnPosition == Vector3.zero)
            {
                Debug.LogWarning("유효하지 않은 시민 스폰 위치로 인해 스폰을 건너뜁니다.");
                return;
            }

            GameObject newCitizen = Instantiate(
                citizenPrefab,
                spawnPosition,
                Quaternion.Euler(slimeTrans.position - spawnPosition)
            );
            newCitizen.transform.SetParent(parentTransforms[1]);
            currentCitizenList[i].Add(newCitizen);

            NPCBase citizenComponent = newCitizen.GetComponent<NPCBase>();
            if (citizenComponent != null)
            {
                citizenComponent.OnDestroyed += () =>
                {
                    if (this != null && newCitizen != null && !isSceneClosing && Application.isPlaying)
                    {
                        SpawnCitizens(i);
                        currentCitizenList[i].Remove(newCitizen);
                    }
                };
            }
        }
    }


    // 시민 수 관리 (시민이 사라졌을 때 다시 스폰)
    private void ManageCitizenSpawn(int i)
    {
        if (currentCitizenList[i].Count < maxCitizen[i])
        {
            SpawnCitizens(i);
        }
    }

    private void RemoveAllCitizens(int i)
    {
        foreach (GameObject citizen in currentCitizenList[i])
        {

            DestroyImmediate(citizen);
        }
        currentCitizenList[i].Clear();
    }
    #endregion

    #region 골드
    // 랜덤한 위치
    private Vector3 GetRandomGoldSpawnPosition()
    {
        Vector3 spawnPosition = Vector3.zero;
        int maxAttempts = 10; // 유효한 위치를 찾기 위한 최대 시도 횟수
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            // 맵 전체에서 랜덤 위치를 계산
            float randomX = Random.Range(-500, 500);
            float randomZ = Random.Range(-500, 500);

            // 랜덤 위치 계산
            Vector3 randomPosition = new Vector3(randomX, spawnHeight + 10f, randomZ); // 위에서 아래로 레이캐스트

            // Raycast로 surface 레이어 위의 위치를 찾음
            if (Physics.Raycast(randomPosition, Vector3.down, out RaycastHit hit, 20f, surfaceLayerMask))
            {
                spawnPosition = hit.point; // 충돌한 지점을 스폰 위치로 설정
                spawnPosition.y += 0.1f; // 약간 띄워서 스폰 (필요에 따라 조정 가능)
                break;
            }

            attempts++;
        }

        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("유효한 골드 스폰 위치를 찾을 수 없습니다.");
        }

        return spawnPosition;
    }

    // 랜덤한 위치가 없을 때 다시 찾는 함수
    private Vector3 GetValidGoldSpawnPosition()
    {
        Vector3 spawnPosition = Vector3.zero;
        int maxAttempts = 10; // 유효한 위치를 찾기 위한 최대 시도 횟수
        int attempts = 0;
        //float maxNavMeshDistance = 10f; // 네비매쉬 표면과의 최대 거리 (네비매쉬 위치 샘플링 시 사용)

        while (attempts < maxAttempts)
        {  
            // 랜덤한 위치를 생성
            Vector3 randomPosition = GetRandomGoldSpawnPosition();
            
            // 랜덤한 위치를 생성
            NNInfo nearestNodeInfo = AstarPath.active.GetNearest(randomPosition);
            if (nearestNodeInfo.node != null && nearestNodeInfo.node.Walkable)
            {
                spawnPosition = nearestNodeInfo.position;
                spawnPosition.y = 0.9f; // Adjust height
                break;
            }
        }
     
        return spawnPosition;
    }
    // 골드 오브젝트 풀링을 사용한 스폰
    private GameObject GetGoldFromPool()
    {
        if (goldPool.Count > 0)
        {
            GameObject gold = goldPool.Dequeue();
            gold.SetActive(true);
            return gold;
        }
        else
        {
            // 새로 생성된 골드에 이벤트가 중복 등록되지 않도록 주의
            GameObject newGold = Instantiate(goldPrefab);
            newGold.transform.SetParent(parentTransforms[2]);

            Gold goldComponent = newGold.GetComponent<Gold>();
            if (goldComponent != null)
            {
                goldComponent.OnDisableGold -= ReturnGoldToPool; // 중복 등록 방지
                goldComponent.OnDisableGold += ReturnGoldToPool; // 이벤트 등록
            }

            return newGold;
        }
    }

    private void ReturnGoldToPool(GameObject gold)
    {
        if (gold == null || gold.Equals(null))
        {
            return;
        }

        if (!goldPool.Contains(gold))
        {
            gold.SetActive(false);
            goldPool.Enqueue(gold);
            activeGoldList.Remove(gold); // 리스트에서도 제거
        }
    }

    private void SpawnGold()
    {
        if (slimeTrans == null)
            return;

        Vector3 spawnPosition = GetRandomGoldSpawnPosition();

        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("골드 스폰 실패: 유효한 위치를 찾지 못했습니다.");
            return;
        }

        GameObject gold = GetGoldFromPool();
        if (activeGoldList.Contains(gold))
        {
            Debug.LogWarning("중복 활성화된 골드가 발견되었습니다.");
            return;
        }

        gold.transform.position = spawnPosition;
        gold.transform.rotation = Quaternion.identity;
        activeGoldList.Add(gold);
    }


    private void RemoveAllGold()
    {
        // 리스트 복사 후 반복문에서 제거하여 예외 방지
        List<GameObject> goldListCopy = new List<GameObject>(activeGoldList);
        foreach (GameObject gold in goldListCopy)
        {
            if (gold != null && !gold.Equals(null))
            {
                ReturnGoldToPool(gold);
            }
        }
        activeGoldList.Clear();
    }
    #endregion


    // 기즈모로 생성 범위를 시각적으로 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // 기즈모 색상을 빨간색으로 설정
        Gizmos.DrawWireSphere(transform.position, spawnRadius); // 생성 범위를 원으로 표시
       
        for (int i = 0; i < citizenSpawnTransforms.Length; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(citizenSpawnTransforms[i].position, citizenSpawnRange[i]);
        }
    }
}

[System.Serializable]
public struct SpawnObjectsList
{
    [LabelText("단계별 스폰되는 오브젝트")]
    public GameObject[] SpawnObjects;

    [FoldoutGroup("폭격기"), LabelText("폭격기 On"), Space(10f)]
    public bool isBomber;
    [FoldoutGroup("폭격기"), LabelText("스폰 주기(초)")]
    public float spawnInterval;
}