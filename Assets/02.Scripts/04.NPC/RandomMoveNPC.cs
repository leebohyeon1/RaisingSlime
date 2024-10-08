using UnityEngine;
using UnityEngine.AI;

public class RandomMoveNPC : NPCBase
{
    [SerializeField]
    private float randomRange = 10f; // 무작위 위치를 설정할 범위
    private Vector3 currentDestination;

    protected override void Start()
    {
        //base.Start();
        SetRandomDestination(); // 처음 스폰될 때 무작위 위치 설정
    }

    protected override void Update()
    {
        //base.Update();
        agent.SetDestination(currentDestination);
        // 현재 목적지에 도착하면 새로운 무작위 위치 설정
        if (HasReachedDestination())
        {
            SetRandomDestination();
        }
    }

    // 무작위 목적지 설정
    private void SetRandomDestination()
    {
        // NPC의 현재 위치를 기준으로 무작위 목적지 설정
        Vector3 randomDirection = Random.insideUnitSphere * randomRange;
        randomDirection += transform.position;

        NavMeshHit hit;
        // 무작위 위치를 NavMesh 상에서 유효한 위치로 샘플링
        if (NavMesh.SamplePosition(randomDirection, out hit, randomRange, LayerMask.NameToLayer("SideWalk")))
        {
            currentDestination = hit.position;
            agent.SetDestination(currentDestination);
        }
    }

    // 목적지에 도달했는지 확인
    private bool HasReachedDestination()
    {
        // NavMeshAgent의 남은 경로 거리가 0.5 미만이면 도착했다고 간주
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(currentDestination, 1f);
    }
}
