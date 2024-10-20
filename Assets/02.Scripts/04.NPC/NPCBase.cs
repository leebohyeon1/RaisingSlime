using Pathfinding;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NPCBase : MonoBehaviour, IUpdateable
{
    public delegate void DestroyedHandler();
    public event DestroyedHandler OnDestroyed;

    protected AIDestinationSetter aiDestinationSetter;
    protected RichAI richAI;
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
        aiDestinationSetter = GetComponent<AIDestinationSetter>();
        aiDestinationSetter.enabled = true;

        richAI = GetComponent<RichAI>();
        richAI.maxSpeed = moveSpeed;

        // ���� �� �ִ� ������Ʈ
        eatAbleObjectBase = GetComponent<EatAbleObjectBase>();

       
    }

    protected virtual void Start()
    {
        if(target == null)
        {
            target = FindFirstObjectByType<Player>().transform; 
        }

        aiDestinationSetter.target = target;
        
        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    public virtual void OnUpdate(float dt)
    {
        enemyAction();
    }
    
    protected virtual void enemyAction() // �� ������ �ൿ �θ� �Լ�
    {    
        // �⺻�� �÷��̾� ���� �� 
        if (eatAbleObjectBase.GetEaten() || target == null || isExplosion) 
        {
            richAI.enabled = false;
            return;
        }
        else
        {
            richAI.enabled = true;

            MoveToTarget();
        }

    }

    protected virtual Vector3 TargetGroundPos() // �÷��̾� ������ ��
    {
        //�÷��̾� ����� ��� Ŀ���� ������ y�� ���� 0���� �ʱ�ȭ

        if (target == null)
            return Vector3.zero;

        return new Vector3(target.position.x, 0.0f, target.position.z);
    }

    protected virtual Vector3 TargetPosSameYPos() // �÷��̾� ������ ��
    {
        if (target == null)
            return Vector3.zero;

        //�÷��̾� ����� ��� Ŀ���� ������ y�� ���� 0���� �ʱ�ȭ
        return new Vector3(target.position.x, transform.position.y, target.position.z);
    }

    public virtual void SetTarget(Transform transform) // Ÿ�� ����
    {
        target = transform;
    }

    protected virtual void MoveToTarget() { }

    protected virtual IEnumerator OnAgent()
    {
        yield return new WaitForSeconds(1f);

        richAI.enabled = true;
        GetComponent<Rigidbody>().isKinematic = true;
        isExplosion = false;
        
    }

    public virtual void Explosion()
    {
        richAI.enabled = false;
        GetComponent<Rigidbody>().isKinematic = false;
        isExplosion = true;

        StartCoroutine(OnAgent());
    }

    protected virtual void OnDestroy()
    {
        GameLogicManager.Instance.DeregisterUpdatableObject(this);
        
        OnDestroyed?.Invoke(); // ���� �ı��� �� �̺�Ʈ �߻�
    }
}
