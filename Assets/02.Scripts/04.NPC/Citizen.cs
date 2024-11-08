using Pathfinding;
using UnityEngine;
using UnityEngine.AI;

public class Citizen : NPCBase
{
    public float wanderRadius = 10f;   // ��ȸ �ݰ�
    public float wanderTimer = 5f;     // ���ο� �������� �̵��ϴ� �ð� ����

    private float timer;

    IAstarAI ai;
    public float radius = 20;

    protected override void Start()
    {
        timer = wanderTimer;

        ai = GetComponent<IAstarAI>();

        GameLogicManager.Instance.RegisterUpdatableObject(this);

        transform.localRotation = Quaternion.identity;
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

        //timer += Time.deltaTime;

        //// Ÿ�̸Ӱ� ������ �ð��� �ʰ��ϸ� ���ο� �������� �̵�
        //if (timer >= wanderTimer)
        //{
        //    Vector3 newPos = GetValidPosition();
        //    aiDestinationSetter.target.position = newPos;
        //    timer = 0;
        //}
        
        if (!ai.pathPending && (ai.reachedEndOfPath || !ai.hasPath))
        {
            ai.destination = GetValidPosition();
            ai.SearchPath();
        }
    }

    Vector3 PickRandomPoint()
    {
        var point = Random.insideUnitSphere * radius;

        point.y = 0;
        point += ai.position;
        return point;
    }

    // ��ȿ�� ��ġ�� ã�� �Լ�
    private Vector3 GetValidPosition()
    {
        Vector3 newPos;
        do
        {
            newPos = PickRandomPoint();
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
