using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NPCBase : MonoBehaviour
{
    protected NavMeshAgent agent;
    protected EatAbleObjectBase eatAbleObjectBase;

    protected Transform target; // �÷��̾� ��ġ

    [BoxGroup("�⺻"), LabelText("������ NPC")]
    public bool isEnemy = false;
    [BoxGroup("�⺻"), LabelText("�̵� �ӵ�"),SerializeField]
    protected float moveSpeed = 4f;
    [BoxGroup("�⺻"), LabelText("�浹 ������"), SerializeField]
    public float collisionDamage = 1f;

    [BoxGroup("�⺻"), LabelText("�׺�Ž� ǥ��"), SerializeField]
    private Transform surface;


    protected virtual void Awake()
    {
        //Nav Mesh Agent �ʱ�ȭ
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.enabled = false;

        // surface ����
        surface.GetComponent<NavMeshSurface>().BuildNavMesh();
        agent.enabled = true;

        // ���� �� �ִ� ������Ʈ
        eatAbleObjectBase = GetComponent<EatAbleObjectBase>();
    }
    protected virtual void Start()
    {
        if(target == null)
        {
            target = FindFirstObjectByType<Player>().transform;
            Debug.Log("find Player"); 
        }
        
    }

    protected virtual void Update()
    {
        enemyAction();
    }

    protected virtual void enemyAction() // �� ������ �ൿ �θ� �Լ�
    {
        // �⺻�� �÷��̾� ���� �� 
        if (eatAbleObjectBase.GetEaten()) 
        {
            return;
        }
        CheckNavMesh();

        MoveToTarget();
    }

    protected virtual Vector3 TargetGroundPos() // �÷��̾� ������ ��
    {
        //�÷��̾� ����� ��� Ŀ���� ������ y�� ���� 0���� �ʱ�ȭ
        return new Vector3(target.position.x, 0.0f, target.position.z);
    }

    protected virtual Vector3 TargetPosSameYPos() // �÷��̾� ������ ��
    {
        //�÷��̾� ����� ��� Ŀ���� ������ y�� ���� 0���� �ʱ�ȭ
        return new Vector3(target.position.x, transform.position.y, target.position.z);
    }

    public virtual void SetTarget(Transform transform) // Ÿ�� ����
    {
        target = transform;
    }

    protected virtual void CheckNavMesh(float distance = 9f) 
    {
        if (Vector3.Distance(transform.position, surface.position) > distance)
        {
            surface.transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
            surface.GetComponent<NavMeshSurface>().BuildNavMesh();
        }
    }

    protected virtual void MoveToTarget()
    {
        // Ÿ�� ��ġ���� ���� ����� ��ȿ�� NavMesh ��ġ�� ã�´�.
        NavMeshHit hit;
        Vector3 targetPosition = TargetGroundPos();

        // ��ȿ�� NavMesh ��ġ�� ã���� �� ��ġ�� �̵�
        if (NavMesh.SamplePosition(targetPosition, out hit, 11.0f, NavMesh.AllAreas))
        {
            targetPosition = hit.position;
        }

        // NavMesh ���� ��ȿ�� ��ġ�� �̵� �õ�
        if (!agent.SetDestination(targetPosition))
        {
            // ��� ���� ���� ��, Ÿ�� �������� ���� �Ÿ��� ���� �̵�
            Vector3 direction = (TargetPosSameYPos() - transform.position).normalized;
            Vector3 fallbackPosition = transform.position + direction * 5f; // 5�� Ÿ�� �������� �̵��� �Ÿ�

            agent.SetDestination(fallbackPosition);
        }
    }

}
