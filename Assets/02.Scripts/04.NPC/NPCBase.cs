using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NPCBase : MonoBehaviour
{
    public delegate void DestroyedHandler();
    public event DestroyedHandler OnDestroyed;

    protected NavMeshAgent agent;
    protected EatAbleObjectBase eatAbleObjectBase;

    protected Transform target; // �÷��̾� ��ġ

    [BoxGroup("�⺻"), LabelText("������ NPC")]
    public bool isEnemy = false;
    [BoxGroup("�⺻"), LabelText("�̵� �ӵ�"),SerializeField]
    protected float moveSpeed = 4f;
    [BoxGroup("�⺻"), LabelText("�浹 ������"), SerializeField]
    public float collisionDamage = 1f;

    public bool isExplosion = false;

    protected virtual void Awake()
    {                
        //Nav Mesh Agent �ʱ�ȭ
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = true;   
        agent.speed = moveSpeed;

        // ���� �� �ִ� ������Ʈ
        eatAbleObjectBase = GetComponent<EatAbleObjectBase>();
    }

    protected virtual void Start()
    {
        if(target == null)
        {
            target = FindFirstObjectByType<Player>().transform; 
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

        if (isExplosion)
        {
           
            return;
        }

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

    protected virtual IEnumerator OnAgent()
    {
        yield return new WaitForSeconds(1f);

        agent.enabled = true;
        GetComponent<Rigidbody>().isKinematic = true;
        isExplosion = false;
        
    }

    public virtual void Explosion()
    {
        agent.enabled = false;
        GetComponent<Rigidbody>().isKinematic = false;
        isExplosion = true;

        StartCoroutine(OnAgent());
    }

    protected virtual void OnDestroy()
    {
        if (OnDestroyed != null)
        {
            OnDestroyed.Invoke(); // ���� �ı��� �� �̺�Ʈ �߻�
        }
    }
}
