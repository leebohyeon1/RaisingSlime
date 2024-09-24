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

    protected Transform target; // 플레이어 위치

    [BoxGroup("기본"), LabelText("공격형 NPC")]
    public bool isEnemy = false;
    [BoxGroup("기본"), LabelText("이동 속도"),SerializeField]
    protected float moveSpeed = 4f;
    [BoxGroup("기본"), LabelText("충돌 데미지"), SerializeField]
    public float collisionDamage = 1f;

    [BoxGroup("기본"), LabelText("네비매쉬 표면"), SerializeField]
    private Transform surface;


    protected virtual void Awake()
    {
        //Nav Mesh Agent 초기화
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.enabled = false;

        // surface 생성
        surface.GetComponent<NavMeshSurface>().BuildNavMesh();
        agent.enabled = true;

        // 먹을 수 있는 오브젝트
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

    protected virtual void enemyAction() // 적 종류별 행동 부모 함수
    {
        // 기본은 플레이어 따라 감 
        if (eatAbleObjectBase.GetEaten()) 
        {
            return;
        }
        CheckNavMesh();

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

}
