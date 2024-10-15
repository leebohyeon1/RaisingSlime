using UnityEngine;
using UnityEngine.AI;

public class Citizen : NPCBase
{
    public float wanderRadius = 10f;   // ��ȸ �ݰ�
    public float wanderTimer = 5f;     // ���ο� �������� �̵��ϴ� �ð� ����

    private float timer;

    protected override void Start()
    {
        timer = wanderTimer;

        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    protected override void enemyAction()
    {  
        // �⺻�� �÷��̾� ���� �� 
        if (eatAbleObjectBase.GetEaten())
        {
            return;
        }

        if (isExplosion)
        {

            return;
        }

        timer += Time.deltaTime;

        // Ÿ�̸Ӱ� ������ �ð��� �ʰ��ϸ� ���ο� �������� �̵�
        if (timer >= wanderTimer)
        {
            Vector3 newPos = GetValidPosition();
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    // ��ȿ�� ��ġ�� ã�� �Լ�
    private Vector3 GetValidPosition()
    {
        Vector3 newPos;
        do
        {
            newPos = RandomNavSphere(transform.position, wanderRadius, NavMesh.AllAreas);
        } while (Vector3.Dot(transform.forward, (newPos - transform.position).normalized) < 0);

        return newPos;
    }

    // NavMesh �ȿ��� ������ ��ġ�� ã�� �Լ�
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }

}
