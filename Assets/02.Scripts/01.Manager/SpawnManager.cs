using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    public int step = 0;
    public SpawnObjectsList[] spawnObjectsList;

    public float spawnRadius = 50f; // ���� �ݰ� (�� �ۿ��� �󸶳� ������ ���� ���� �������� ����)
    public float spawnHeight = 3f; // ���� ������ ���� ����

    private List<GameObject> currentStepEnemyList = new List<GameObject>(); // ���� ���ܿ��� ������ �� ����Ʈ
    private List<GameObject> beforeStepEnemyList = new List<GameObject>(); // ������ ������ �� ����Ʈ
    private bool isSceneClosing = false; // ���� ������ ������ ���θ� Ȯ���ϴ� �÷���

    private Transform slimeTrans;

    private void Start()
    {
        slimeTrans = FindFirstObjectByType<Player>().transform;

        SpawnEnemiesForCurrentStep();
    }

    void Update()
    {
        // �����Ŵ����� ��ġ�� �÷��̾�(������) ��ġ�� ����
        transform.position = new Vector3(slimeTrans.position.x, 0, slimeTrans.position.z);

    }

    // ���� ������ ������ �����ϴ� �Լ�
    private void SpawnEnemiesForCurrentStep()
    {
        // ���� ���ܿ� �ִ� ���� ����
        foreach (GameObject enemyPrefab in spawnObjectsList[step].SpawnObjects)
        {
            SpawnEnemy(enemyPrefab);
        }
    }

    // ���� ���� �� ȣ��Ǵ� �Լ�
    [Button]
    public void ChangeStep(int newStep)
    {
        // ���� ������ ������ ��� beforeStepEnemyList�� �߰�
        foreach (GameObject enemy in currentStepEnemyList)
        {
            if (enemy != null)
            {
                beforeStepEnemyList.Add(enemy); // ���� ������ ������ ���
            }
        }

        // ���� ���� �� ����Ʈ �ʱ�ȭ
        currentStepEnemyList.Clear();

        // ���ο� �������� ����
        step = newStep;

        // ���ο� ������ ������ ����
        SpawnEnemiesForCurrentStep();
    }

    // ���� �����ϴ� �Լ�
    private void SpawnEnemy(GameObject enemyPrefab)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // ������ ���� ���� ���� ����Ʈ�� �߰�
        currentStepEnemyList.Add(newEnemy);

        // ���� �ı��Ǿ��� �� �ٽ� �������� �ʵ��� ���� ������ ������ Ȯ��
        EnemyBase enemyComponent = newEnemy.GetComponent<EnemyBase>();
        if (enemyComponent != null)
        {
            enemyComponent.OnDestroyed += () =>
            {
                // ���� �ı��� ���Ŀ��� �����Ŵ����� �ı����� �ʾҴ��� Ȯ��
                if (this != null && newEnemy != null && !isSceneClosing)
                {

                    // ���� ���� ���ܿ� ������ ������ �ٽ� ����
                    if (!beforeStepEnemyList.Contains(newEnemy))
                    {
                        SpawnEnemy(enemyPrefab); // �ٽ� ���� (���� ������ ����)
                    }

                    // ���� �ı��Ǿ��� �� ����Ʈ���� ����
                    RemoveEnemyFromList(newEnemy);
                }
            };
        }
    }

    // ����Ʈ���� �� ���� �Լ�
    private void RemoveEnemyFromList(GameObject enemy)
    {
        // ���� ���� ����Ʈ���� ����
        if (currentStepEnemyList.Contains(enemy))
        {
            currentStepEnemyList.Remove(enemy);
        }

        // ���� ���� ����Ʈ���� ���� (���� ���� ���)
        if (beforeStepEnemyList.Contains(enemy))
        {
            beforeStepEnemyList.Remove(enemy);
        }
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

    // ���� ���� �� ������ �ߴ��ϱ� ���� �÷��� ����
    private void OnApplicationQuit()
    {
        isSceneClosing = true;
    }

    private void OnDestroy()
    {
        isSceneClosing = true; // ������Ʈ�� �ı��� ���� �÷��� ����
    }


    // ������ ���� ������ �ð������� ǥ��
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // ����� ������ ���������� ����
        Gizmos.DrawWireSphere(transform.position, spawnRadius); // ���� ������ ������ ǥ��
    }
}

[System.Serializable]
public struct SpawnObjectsList
{
    [LabelText("�ܰ躰 �����Ǵ� ������Ʈ")]
    public GameObject[] SpawnObjects;
}