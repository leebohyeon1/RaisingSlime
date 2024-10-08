using UnityEngine;
using UnityEngine.AI;

public class RandomMoveNPC : NPCBase
{
    [SerializeField]
    private float randomRange = 10f; // 무작위 위치를 설정할 범위
    private Vector3 currentDestination;
    //int sideWalkArea = NavMesh.GetAreaFromName("SideWalk");

    public bool isTurnNext;

    protected override void Start()
    {
        SetRandomDestination(); // 처음 스폰될 때 무작위 위치 설정
    }

    protected override void Update()
    {
        agent.SetDestination(currentDestination);
         
        
    }

    private void FixedUpdate()
    {
        // 현재 목적지에 도착하면 새로운 무작위 위치 설정
        if (HasReachedDestination() && isTurnNext)
        {
            // 인도 밖이면 좌우 회전
            TurnLeftOrRight();
        }
        else if (HasReachedDestination() && !isTurnNext)
        {
            SetRandomDestination();
        }
    }

    // 무작위 목적지 설정
    private void SetRandomDestination()
    {
        // NPC의 전방 방향을 기준으로 무작위 목적지 설정
        Vector3 randomDirection = transform.forward * randomRange; // 전방 방향으로 무작위 거리만큼 이동
        randomDirection += transform.position;

        NavMeshHit hit;
        // 무작위 위치를 NavMesh 상에서 유효한 위치로 샘플링
        if (NavMesh.SamplePosition(randomDirection, out hit, randomRange, NavMesh.AllAreas))
        {
            currentDestination = hit.position;

            // 목적지가 인도(SideWalk) 레이어에 있는지 확인
            if (!IsOnSideWalk(currentDestination))
            {
                // 인도 밖이면 좌우 회전
                NavMesh.SamplePosition(transform.forward * 10, out hit, randomRange, NavMesh.AllAreas);
                currentDestination = hit.position;
                isTurnNext = true;
            }
            else
            {
                isTurnNext = false;
            }
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

    // 목적지가 인도(SideWalk) 레이어에 있는지 확인하는 함수
    private bool IsOnSideWalk(Vector3 position)
    {
        NavMeshHit hit;
        // 해당 위치에서 NavMesh 상의 유효한 위치를 샘플링
        bool isOnNavMesh = NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas);

        // 디버그용 레이 그리기
        Debug.DrawRay(position, Vector3.down * 2, isOnNavMesh ? Color.green : Color.red, 1.0f);

        int sideWalkArea = NavMesh.GetAreaFromName("SideWalk");

        if (isOnNavMesh && hit.mask == (1 << sideWalkArea))
        {
            return true; // NavMesh 상에 있고, SideWalk 영역에 해당하면 true 반환
        }

        return false; // 아니면 false 반환
    }

    // NPC를 오른쪽 또는 왼쪽으로 회전시키는 함수 (오른쪽을 먼저 확인)
    private void TurnLeftOrRight()
    {
        Vector3 rightDirection = transform.right * 5;
        Vector3 leftDirection = -transform.right * 5;

        NavMeshHit hit;
        int sideWalkArea = NavMesh.GetAreaFromName("SideWalk");
        // 오른쪽 먼저 확인
        if (NavMesh.SamplePosition(transform.position + rightDirection, out hit, 10, sideWalkArea))
        {
            // 오른쪽에 길이 있으면 90도 오른쪽 회전
            transform.Rotate(0, 90f, 0);
            SetRandomDestination();
        }
        // 왼쪽 확인
        else if (NavMesh.SamplePosition(transform.position + leftDirection, out hit, 10, sideWalkArea))
        {
            // 왼쪽에 길이 있으면 90도 왼쪽 회전
            transform.Rotate(0, -90f, 0);
            SetRandomDestination();
        }
        else
        {
            // 양쪽 다 길이 없으면 180도 회전
            transform.Rotate(0, 180f, 0);
            SetRandomDestination();
        }

        // 새로운 전방 방향으로 목적지 설정
        agent.SetDestination(currentDestination);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(currentDestination, 1f);
    }
}
