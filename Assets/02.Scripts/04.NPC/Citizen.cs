using Pathfinding;
using UnityEngine;
using UnityEngine.AI;

public class Citizen : NPCBase
{
    public float wanderRadius = 10f;   // 배회 반경
    public float wanderTimer = 5f;     // 새로운 목적지로 이동하는 시간 간격

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
        // 기본은 플레이어 따라 감 
        if (eatAbleObjectBase.GetEaten())
        {
            return;
        }

        if (isExplosion)
        {

            return;
        }

        //timer += Time.deltaTime;

        //// 타이머가 설정된 시간을 초과하면 새로운 목적지로 이동
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

    // 유효한 위치를 찾는 함수
    private Vector3 GetValidPosition()
    {
        Vector3 newPos;
        do
        {
            newPos = PickRandomPoint();
        } while (Vector3.Dot(transform.forward, (newPos - transform.position).normalized) < 0);

        return newPos;
    }

    // NavMesh 안에서 무작위 위치를 찾는 함수
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }

}
