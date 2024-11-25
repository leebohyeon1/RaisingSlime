using Pathfinding;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    
    private float maxDistance = 48f; // 타겟과의 최대 거리

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

            CheckDistanceToTarget();
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
        if(target == null)
        {
            return;
        }

        aiPath.destination = TargetGroundPos();
        aiPath.SearchPath();
    }

    protected void CheckDistanceToTarget()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, TargetGroundPos());

        if (distanceToTarget > maxDistance)
        {
            // 타겟이 그리드 밖에 있으므로 AIPath와 콜라이더를 비활성화하고 직접 이동
            aiPath.enabled = false;
            Collider npcCollider = GetComponent<Collider>();
            if (npcCollider != null)
            {
                npcCollider.enabled = false;
            }

            DirectMoveToPlayer();
        }
        else
        {
            // 타겟이 그리드 내에 있으며 maxDistance 이하이므로 길찾기 가능
            aiPath.enabled = true;
            Collider npcCollider = GetComponent<Collider>();
            if (npcCollider != null)
            {
                npcCollider.enabled = true;
            }
        }
    }

    private void DirectMoveToPlayer()
    {
        if (target == null) return;

        Vector3 direction = (TargetPosSameYPos() - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        transform.rotation = Quaternion.LookRotation(TargetPosSameYPos());
    }


    protected void TeleportToClosestNode()
    {
        Vector3 dir = (TargetPosSameYPos() - transform.position).normalized;

        // 현재 transform.position을 기준으로 가장 가까운 네비게이션 노드를 찾음
        NNInfo nearestNode = AstarPath.active.GetNearest(transform.position);
        if (nearestNode.node != null)
        {
            // 가장 가까운 노드의 위치로 텔레포트
            aiPath.Teleport(nearestNode.position + (dir * 5));
            aiPath.SearchPath();
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
