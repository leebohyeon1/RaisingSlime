using UnityEngine;
using UnityEngine.AI;

public class RandomMoveNPC : NPCBase
{
    [SerializeField]
    private float randomRange = 10f; // ������ ��ġ�� ������ ����
    private Vector3 currentDestination;

    protected override void Start()
    {
        //base.Start();
        SetRandomDestination(); // ó�� ������ �� ������ ��ġ ����
    }

    protected override void Update()
    {
        //base.Update();
        agent.SetDestination(currentDestination);
        // ���� �������� �����ϸ� ���ο� ������ ��ġ ����
        if (HasReachedDestination())
        {
            SetRandomDestination();
        }
    }

    // ������ ������ ����
    private void SetRandomDestination()
    {
        // NPC�� ���� ��ġ�� �������� ������ ������ ����
        Vector3 randomDirection = Random.insideUnitSphere * randomRange;
        randomDirection += transform.position;

        NavMeshHit hit;
        // ������ ��ġ�� NavMesh �󿡼� ��ȿ�� ��ġ�� ���ø�
        if (NavMesh.SamplePosition(randomDirection, out hit, randomRange, LayerMask.NameToLayer("SideWalk")))
        {
            currentDestination = hit.position;
            agent.SetDestination(currentDestination);
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(currentDestination, 1f);
    }
}
