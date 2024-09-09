using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab; // 적 프리팹
    public float spawnRadius = 50f; // 생성 반경 (맵 밖에서 얼마나 떨어진 곳에 적을 생성할지 결정)
    public float spawnHeight = 3f; // 적이 생성될 때의 높이

    [Header("Spawn Frequency")]
    public float spawnInterval = 5f; // 적 생성 주기 (초 단위)
    private float spawnTimer = 0f; // 적 생성 타이머

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        //GameManager.Instance.enemyList.Add(enemy);
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

    // 기즈모로 생성 범위를 시각적으로 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // 기즈모 색상을 빨간색으로 설정
        Gizmos.DrawWireSphere(transform.position, spawnRadius); // 생성 범위를 원으로 표시
    }
}
