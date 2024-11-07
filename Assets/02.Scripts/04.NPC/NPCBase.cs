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

   // protected AIDestinationSetter aiDestinationSetter;
    protected AIPath aiPath;
    protected EatAbleObjectBase eatAbleObjectBase;

    protected Transform target; // �÷��̾� ��ġ

    [BoxGroup("기본"), LabelText("공격형 NPC")]
    public bool isEnemy = false;
    [BoxGroup("기본"), LabelText("이동 속도"),SerializeField]
    protected float moveSpeed = 4f;
    [BoxGroup("기본"), LabelText("충돌 데미지"), SerializeField]
    public float collisionDamage = 1f;

    public bool isExplosion = false;

    protected virtual void Awake()
    {                
        //aiDestinationSetter = GetComponent<AIDestinationSetter>();
        //aiDestinationSetter.enabled = true;

        aiPath = GetComponent<AIPath>();

        aiPath.maxSpeed = moveSpeed;

        eatAbleObjectBase = GetComponent<EatAbleObjectBase>();

       
    }

    protected virtual void Start()
    {
        if(target == null)
        {
            target = FindFirstObjectByType<Player>().transform; 
        }

        //aiDestinationSetter.target = target;
        
        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    public virtual void OnUpdate(float dt)
    {
        enemyAction();
    }
    
    protected virtual void enemyAction() 
    {    
        if (eatAbleObjectBase.GetEaten() || target == null || isExplosion) 
        {
            aiPath.enabled = false;
            return;
        }
        else
        {
            aiPath.enabled = true;

            MoveToTarget();
        }

    }

    protected virtual Vector3 TargetGroundPos() 
    {

        if (target == null)
            return Vector3.zero;

        return new Vector3(target.position.x, 0.0f, target.position.z);
    }

    protected virtual Vector3 TargetPosSameYPos() 
    {
        if (target == null)
            return Vector3.zero;

        return new Vector3(target.position.x, transform.position.y, target.position.z);
    }

    public virtual void SetTarget(Transform transform) // Ÿ�� ����
    {
        target = transform;
    }

    protected virtual void MoveToTarget() 
    {
        if(target != null && aiPath != null)
        {
            aiPath.destination = TargetGroundPos();
        }

    
    }

    protected virtual IEnumerator OnAgent()
    {
        yield return new WaitForSeconds(1f);

        aiPath.enabled = true;
        GetComponent<Rigidbody>().isKinematic = true;
        isExplosion = false;
        
    }

    public virtual void Explosion()
    {
        aiPath.enabled = false;
        GetComponent<Rigidbody>().isKinematic = false;
        isExplosion = true;

        StartCoroutine(OnAgent());
    }

    protected virtual void OnDestroy()
    {
        if (GameLogicManager.Instance != null)
        {
            GameLogicManager.Instance.DeregisterUpdatableObject(this);
        }

        OnDestroyed?.Invoke();
    }
}
