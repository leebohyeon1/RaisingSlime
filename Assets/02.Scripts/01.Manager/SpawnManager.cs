using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [BoxGroup("점수 세팅"), LabelText("점수별 단계"), SerializeField]
    private int[] stepByScore;

    [BoxGroup("적 세팅"), LabelText("현재 레벨"), SerializeField]
    private int step = 0;
    [BoxGroup("적 세팅"), LabelText("스폰하는 적 리스트"), SerializeField]
    private SpawnObjectsList[] spawnObjectsList;

    [BoxGroup("위치 세팅"), LabelText("스폰 반경"), SerializeField]
    private float spawnRadius = 50f; // 생성 반경 (맵 밖에서 얼마나 떨어진 곳에 적을 생성할지 결정)
    [BoxGroup("위치 세팅"), LabelText("스폰 높이"), SerializeField]
    private float spawnHeight = 3f; // 적이 생성될 때의 높이

    [BoxGroup("특수 오브젝트"), LabelText("폭격기"), SerializeField]
    private GameObject bomberPrefab;

    private List<GameObject> currentStepEnemyList = new List<GameObject>(); // 현재 스텝에서 스폰된 적 리스트
    private List<GameObject> beforeStepEnemyList = new List<GameObject>(); // 이전에 스폰된 적 리스트
    private bool isSceneClosing = false; // 씬이 닫히는 중인지 여부를 확인하는 플래그

    private Transform slimeTrans;
    private float bomberSpawnTimer = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (stepByScore.Length != spawnObjectsList.Length)
        {
            Debug.LogError("점수별 단계와 단계별 적 리스트의 길이가 다릅니다.");
        }

        slimeTrans = FindFirstObjectByType<Player>().transform;

        SpawnEnemiesForCurrentStep();
    }

    void Update()
    {
        // 스폰매니저의 위치를 플레이어(슬라임) 위치로 설정
        transform.position = new Vector3(slimeTrans.position.x, 0, slimeTrans.position.z);

        //폭격기 스폰
        SpawnBomber();

        // 점수 체크
        CheckScore();
    }

    // 현재 스텝의 적들을 스폰하는 함수
    private void SpawnEnemiesForCurrentStep()
    {
        // 현재 스텝에 있는 적을 스폰
        foreach (GameObject enemyPrefab in spawnObjectsList[step].SpawnObjects)
        {
            SpawnEnemy(enemyPrefab);
        }
    }

    // 점수를 체크하는 함수
    public void CheckScore()
    {
        // 마지막 단계에서는 아무 효과도 일어나지 않는다.
        if(GameManager.Instance.GetScore() > stepByScore[stepByScore.Length - 1])
        {
            return;
        }

        // 현재 점수가 다음 단계 별 점수보다 크면 다음 단계로 넘어간다.
        if (stepByScore[step] < GameManager.Instance.GetScore())
        {
            step++;
            ChangeStep(step);
        }
    }

    // 스텝 변경 시 호출되는 함수
    [Button]
    public void ChangeStep(int newStep)
    {
        // 현재 스텝의 적들을 모두 beforeStepEnemyList에 추가
        foreach (GameObject enemy in currentStepEnemyList)
        {
            if (enemy != null)
            {
                beforeStepEnemyList.Add(enemy); // 이전 스텝의 적으로 기록
            }
        }

        // 현재 스텝 적 리스트 초기화
        currentStepEnemyList.Clear();

        // 새로운 스텝으로 변경
        step = newStep;

        // 새로운 스텝의 적들을 스폰
        SpawnEnemiesForCurrentStep();
    }

    // 적을 스폰하는 함수
    private void SpawnEnemy(GameObject enemyPrefab)
    {
        Vector3 spawnPosition = GetValidSpawnPosition();

        // 유효한 NavMesh 위의 위치를 찾았을 때만 적을 스폰
        if (spawnPosition != Vector3.zero)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            // 스폰된 적을 현재 스텝 리스트에 추가
            currentStepEnemyList.Add(newEnemy);

            // 적이 파괴되었을 때 다시 스폰하지 않도록 이전 스텝의 적인지 확인
            NPCParent enemyComponent = newEnemy.GetComponent<NPCParent>();
            if (enemyComponent != null)
            {
                enemyComponent.OnDestroyed += () =>
                {
                    // 적이 파괴된 이후에도 스폰매니저가 파괴되지 않았는지 확인
                    if (this != null && newEnemy != null && !isSceneClosing)
                    {
                        // 적이 이전 스텝에 속하지 않으면 다시 스폰
                        if (!beforeStepEnemyList.Contains(newEnemy))
                        {
                            SpawnEnemy(enemyPrefab); // 다시 스폰 (현재 스텝의 적만)
                        }

                        // 적이 파괴되었을 때 리스트에서 제거
                        RemoveEnemyFromList(newEnemy);
                    }
                };

                enemyComponent.SetNPCTarget(slimeTrans);
            }
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
                NPCParent enemyComponent = newEnemy.GetComponent<NPCParent>();
                enemyComponent.SetNPCTarget(slimeTrans);
            }
        }
    }

    // 랜덤한 위치가 없을 때 다시 찾는 함수
    private Vector3 GetValidSpawnPosition()
    {
        Vector3 spawnPosition = Vector3.zero;
        int maxAttempts = 10; // 유효한 위치를 찾기 위한 최대 시도 횟수
        int attempts = 0;
        float spawnCheckRadius = 2f; // 충돌 체크할 반경 (적 크기에 맞게 설정)

        while (attempts < maxAttempts)
        {
            // 랜덤한 위치를 생성
            Vector3 randomPosition = GetRandomSpawnPosition();

            // 충돌체와 겹치지 않는지 확인
            if (!IsPositionOccupied(randomPosition, spawnCheckRadius))
            {
                spawnPosition = randomPosition;
                break;
            }

            attempts++;
        }

        return spawnPosition;
    }

    // position에 충돌하는 것이 있는지 확인
    private bool IsPositionOccupied(Vector3 position, float radius)
    {
        // 주어진 위치에 일정 반경 내에 충돌체가 있는지 확인
        Collider[] colliders = Physics.OverlapSphere(position, radius);
        return colliders.Length > 0; // 만약 충돌체가 하나라도 있으면 위치가 점유된 것으로 판단
    }

    // 랜덤한 위치
    private Vector3 GetRandomSpawnPosition()
    {
        // XZ 평면에서 랜덤한 방향을 선택하여 생성 범위 외부의 랜덤 위치를 계산
        Vector2 randomDirection = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = new Vector3(randomDirection.x, spawnHeight, randomDirection.y);

        // 플레이어 또는 맵 중앙에서 먼 위치에 스폰되도록 설정
        spawnPosition += transform.position;

        return spawnPosition;
    }

    // 리스트에서 적 제거 함수
    private void RemoveEnemyFromList(GameObject enemy)
    {
        // 현재 스텝 리스트에서 제거
        if (currentStepEnemyList.Contains(enemy))
        {
            currentStepEnemyList.Remove(enemy);
        }

        // 이전 스텝 리스트에서 제거 (만약 있을 경우)
        if (beforeStepEnemyList.Contains(enemy))
        {
            beforeStepEnemyList.Remove(enemy);
        }
    }

    // 모든 적을 제거하는 함수
    public void RemoveAllEnemy()
    {
        foreach(GameObject enemy in currentStepEnemyList)
        {
            DestroyImmediate(enemy);
        }
        currentStepEnemyList.Clear();
    
        foreach(GameObject enemy in beforeStepEnemyList)
        {
            DestroyImmediate(enemy);
        }
        beforeStepEnemyList.Clear();
    }

    // 씬이 닫힐 때 스폰을 중단하기 위해 플래그 설정
    private void OnApplicationQuit()
    {
        isSceneClosing = true;
    }

    private void OnDestroy()
    {
        isSceneClosing = true; // 오브젝트가 파괴될 때도 플래그 설정
        RemoveAllEnemy(); // 모든 적 제거

        // 씬이 닫힐 때 불필요한 오브젝트를 정리하여 메모리 관리
        Resources.UnloadUnusedAssets();
    }


    // 기즈모로 생성 범위를 시각적으로 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // 기즈모 색상을 빨간색으로 설정
        Gizmos.DrawWireSphere(transform.position, spawnRadius); // 생성 범위를 원으로 표시
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