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

    protected Transform target; // 플레이어 위치

    [BoxGroup("기본"), LabelText("공격형 NPC")]
    public bool isEnemy = false;
    [BoxGroup("기본"), LabelText("이동 속도"),SerializeField]
    protected float moveSpeed = 4f;
    [BoxGroup("기본"), LabelText("충돌 데미지"), SerializeField]
    public float collisionDamage = 1f;

    public bool isExplosion = false;

    protected virtual void Awake()
    {                
        //Nav Mesh Agent 초기화
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = true;   
        agent.speed = moveSpeed;

        // 먹을 수 있는 오브젝트
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

    protected virtual void enemyAction() // 적 종류별 행동 부모 함수
    {

        // 기본은 플레이어 따라 감 
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

    protected virtual Vector3 TargetGroundPos() // 플레이어 포지션 값
    {
        //플레이어 사이즈가 계속 커지기 때문에 y축 값은 0으로 초기화
        return new Vector3(target.position.x, 0.0f, target.position.z);
    }

    protected virtual Vector3 TargetPosSameYPos() // 플레이어 포지션 값
    {
        //플레이어 사이즈가 계속 커지기 때문에 y축 값은 0으로 초기화
        return new Vector3(target.position.x, transform.position.y, target.position.z);
    }

    public virtual void SetTarget(Transform transform) // 타겟 설정
    {
        target = transform;
    }

    protected virtual void MoveToTarget()
    {
        // 타겟 위치에서 가장 가까운 유효한 NavMesh 위치를 찾는다.
        NavMeshHit hit;
        Vector3 targetPosition = TargetGroundPos();

        // 유효한 NavMesh 위치를 찾으면 그 위치로 이동
        if (NavMesh.SamplePosition(targetPosition, out hit, 11.0f, NavMesh.AllAreas))
        {
            targetPosition = hit.position;
        }

        // NavMesh 상의 유효한 위치로 이동 시도
        if (!agent.SetDestination(targetPosition))
        {
            // 경로 설정 실패 시, 타겟 방향으로 일정 거리를 더해 이동
            Vector3 direction = (TargetPosSameYPos() - transform.position).normalized;
            Vector3 fallbackPosition = transform.position + direction * 5f; // 5는 타겟 방향으로 이동할 거리

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
            OnDestroyed.Invoke(); // 적이 파괴될 때 이벤트 발생
        }
    }
}
