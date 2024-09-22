using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
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

    protected virtual void Start()
    {
        //Nav Mesh Agent �ʱ�ȭ
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;


        eatAbleObjectBase = GetComponent<EatAbleObjectBase>();

        target = FindFirstObjectByType<Player>().transform;
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

        agent.SetDestination(targetGroundPos());
    }

    protected virtual Vector3 targetGroundPos() // �÷��̾� ������ ��
    {
        //�÷��̾� ����� ��� Ŀ���� ������ y�� ���� 0���� �ʱ�ȭ
        return new Vector3(target.position.x, 0.0f, target.position.z);
    }

    protected virtual Vector3 targetPosSameYPos() // �÷��̾� ������ ��
    {
        //�÷��̾� ����� ��� Ŀ���� ������ y�� ���� 0���� �ʱ�ȭ
        return new Vector3(target.position.x, transform.position.y, target.position.z);
    }

    //protected virtual void OnCollisionEnter(Collision collision)
    //{
    //    if(collision.gameObject.CompareTag("Player"))
    //    {
    //        Player player = collision.gameObject.GetComponent<Player>();

    //        player.TakeDamage(attackDamage);
    //    }
    //}


    private void OnDestroy()
    {
        if (OnDestroyed != null)
        {
            OnDestroyed.Invoke(); // ���� �ı��� �� �̺�Ʈ �߻�
        }
    }
}
