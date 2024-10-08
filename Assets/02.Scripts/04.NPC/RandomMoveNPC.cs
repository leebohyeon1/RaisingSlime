using UnityEngine;
using UnityEngine.AI;

public class RandomMoveNPC : NPCBase
{
    [SerializeField]
    private float randomRange = 10f; // ������ ��ġ�� ������ ����
    private Vector3 currentDestination;
    //int sideWalkArea = NavMesh.GetAreaFromName("SideWalk");

    public bool isTurnNext;

    protected override void Start()
    {
        SetRandomDestination(); // ó�� ������ �� ������ ��ġ ����
    }

    protected override void Update()
    {
        agent.SetDestination(currentDestination);
         
        
    }

    private void FixedUpdate()
    {
        // ���� �������� �����ϸ� ���ο� ������ ��ġ ����
        if (HasReachedDestination() && isTurnNext)
        {
            // �ε� ���̸� �¿� ȸ��
            TurnLeftOrRight();
        }
        else if (HasReachedDestination() && !isTurnNext)
        {
            SetRandomDestination();
        }
    }

    // ������ ������ ����
    private void SetRandomDestination()
    {
        // NPC�� ���� ������ �������� ������ ������ ����
        Vector3 randomDirection = transform.forward * randomRange; // ���� �������� ������ �Ÿ���ŭ �̵�
        randomDirection += transform.position;

        NavMeshHit hit;
        // ������ ��ġ�� NavMesh �󿡼� ��ȿ�� ��ġ�� ���ø�
        if (NavMesh.SamplePosition(randomDirection, out hit, randomRange, NavMesh.AllAreas))
        {
            currentDestination = hit.position;

            // �������� �ε�(SideWalk) ���̾ �ִ��� Ȯ��
            if (!IsOnSideWalk(currentDestination))
            {
                // �ε� ���̸� �¿� ȸ��
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

    // �������� �����ߴ��� Ȯ��
    private bool HasReachedDestination()
    {
        // NavMeshAgent�� ���� ��� �Ÿ��� 0.5 �̸��̸� �����ߴٰ� ����
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            return true;
        }
        return false;
    }

    // �������� �ε�(SideWalk) ���̾ �ִ��� Ȯ���ϴ� �Լ�
    private bool IsOnSideWalk(Vector3 position)
    {
        NavMeshHit hit;
        // �ش� ��ġ���� NavMesh ���� ��ȿ�� ��ġ�� ���ø�
        bool isOnNavMesh = NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas);

        // ����׿� ���� �׸���
        Debug.DrawRay(position, Vector3.down * 2, isOnNavMesh ? Color.green : Color.red, 1.0f);

        int sideWalkArea = NavMesh.GetAreaFromName("SideWalk");

        if (isOnNavMesh && hit.mask == (1 << sideWalkArea))
        {
            return true; // NavMesh �� �ְ�, SideWalk ������ �ش��ϸ� true ��ȯ
        }

        return false; // �ƴϸ� false ��ȯ
    }

    // NPC�� ������ �Ǵ� �������� ȸ����Ű�� �Լ� (�������� ���� Ȯ��)
    private void TurnLeftOrRight()
    {
        Vector3 rightDirection = transform.right * 5;
        Vector3 leftDirection = -transform.right * 5;

        NavMeshHit hit;
        int sideWalkArea = NavMesh.GetAreaFromName("SideWalk");
        // ������ ���� Ȯ��
        if (NavMesh.SamplePosition(transform.position + rightDirection, out hit, 10, sideWalkArea))
        {
            // �����ʿ� ���� ������ 90�� ������ ȸ��
            transform.Rotate(0, 90f, 0);
            SetRandomDestination();
        }
        // ���� Ȯ��
        else if (NavMesh.SamplePosition(transform.position + leftDirection, out hit, 10, sideWalkArea))
        {
            // ���ʿ� ���� ������ 90�� ���� ȸ��
            transform.Rotate(0, -90f, 0);
            SetRandomDestination();
        }
        else
        {
            // ���� �� ���� ������ 180�� ȸ��
            transform.Rotate(0, 180f, 0);
            SetRandomDestination();
        }

        // ���ο� ���� �������� ������ ����
        agent.SetDestination(currentDestination);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(currentDestination, 1f);
    }
}
