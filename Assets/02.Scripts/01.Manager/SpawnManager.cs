using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab; // �� ������
    public float spawnRadius = 50f; // ���� �ݰ� (�� �ۿ��� �󸶳� ������ ���� ���� �������� ����)
    public float spawnHeight = 3f; // ���� ������ ���� ����

    [Header("Spawn Frequency")]
    public float spawnInterval = 5f; // �� ���� �ֱ� (�� ����)
    private float spawnTimer = 0f; // �� ���� Ÿ�̸�

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
        // XZ ��鿡�� ������ ������ �����Ͽ� ���� ���� �ܺ��� ���� ��ġ�� ���
        Vector2 randomDirection = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = new Vector3(randomDirection.x, spawnHeight, randomDirection.y);

        // �÷��̾� �Ǵ� �� �߾ӿ��� �� ��ġ�� �����ǵ��� ����
        spawnPosition += transform.position;

        return spawnPosition;
    }

    // ������ ���� ������ �ð������� ǥ��
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // ����� ������ ���������� ����
        Gizmos.DrawWireSphere(transform.position, spawnRadius); // ���� ������ ������ ǥ��
    }
}
