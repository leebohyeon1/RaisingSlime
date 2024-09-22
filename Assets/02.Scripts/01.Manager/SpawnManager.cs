using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    public int step = 0;
    public SpawnObjectsList[] spawnObjectsList;

    public float spawnRadius = 50f; // 생성 반경 (맵 밖에서 얼마나 떨어진 곳에 적을 생성할지 결정)
    public float spawnHeight = 3f; // 적이 생성될 때의 높이

    private List<GameObject> currentStepEnemyList = new List<GameObject>(); // 현재 스텝에서 스폰된 적 리스트
    private List<GameObject> beforeStepEnemyList = new List<GameObject>(); // 이전에 스폰된 적 리스트
    private bool isSceneClosing = false; // 씬이 닫히는 중인지 여부를 확인하는 플래그

    private Transform slimeTrans;

    private void Start()
    {
        slimeTrans = FindFirstObjectByType<Player>().transform;

        SpawnEnemiesForCurrentStep();
    }

    void Update()
    {
        // 스폰매니저의 위치를 플레이어(슬라임) 위치로 설정
        transform.position = new Vector3(slimeTrans.position.x, 0, slimeTrans.position.z);

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
        Vector3 spawnPosition = GetValidNavMeshPosition();

        // 유효한 NavMesh 위의 위치를 찾았을 때만 적을 스폰
        if (spawnPosition != Vector3.zero)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            // 스폰된 적을 현재 스텝 리스트에 추가
            currentStepEnemyList.Add(newEnemy);

            // 적이 파괴되었을 때 다시 스폰하지 않도록 이전 스텝의 적인지 확인
            NPCBase enemyComponent = newEnemy.GetComponent<NPCBase>();
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
            }
        }
    }

    // NavMesh 위의 유효한 스폰 위치를 찾는 함수
    private Vector3 GetValidNavMeshPosition()
    {
        Vector3 spawnPosition = Vector3.zero;
        int maxAttempts = 10; // 유효한 위치를 찾기 위한 최대 시도 횟수
        int attempts = 0;
        float maxDistance = 10f; // 샘플링할 최대 거리

        while (attempts < maxAttempts)
        {
            Vector3 randomPosition = GetRandomSpawnPosition();
            NavMeshHit hit;

            // NavMesh 위의 유효한 위치인지 확인
            if (NavMesh.SamplePosition(randomPosition, out hit, maxDistance, NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
                break;
            }

            attempts++;
        }

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

    private Vector3 GetRandomSpawnPosition()
    {
        // XZ 평면에서 랜덤한 방향을 선택하여 생성 범위 외부의 랜덤 위치를 계산
        Vector2 randomDirection = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = new Vector3(randomDirection.x, spawnHeight, randomDirection.y);

        // 플레이어 또는 맵 중앙에서 먼 위치에 스폰되도록 설정
        spawnPosition += transform.position;

        return spawnPosition;
    }

    // 씬이 닫힐 때 스폰을 중단하기 위해 플래그 설정
    private void OnApplicationQuit()
    {
        isSceneClosing = true;
    }

    private void OnDestroy()
    {
        isSceneClosing = true; // 오브젝트가 파괴될 때도 플래그 설정
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
}