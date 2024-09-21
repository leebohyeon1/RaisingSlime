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

    protected Transform target; // 플레이어 위치

    [BoxGroup("기본"), LabelText("이동 속도"),SerializeField]
    protected float moveSpeed = 4f;
    [BoxGroup("기본"), LabelText("충돌 데미지"), SerializeField]
    public float collisionDamage = 1f;

    protected virtual void Start()
    {
        //Nav Mesh Agent 초기화
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;


        eatAbleObjectBase = GetComponent<EatAbleObjectBase>();

        target = FindFirstObjectByType<Player>().transform;
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

        agent.SetDestination(targetGroundPos());
    }

    protected virtual Vector3 targetGroundPos() // 플레이어 포지션 값
    {
        //플레이어 사이즈가 계속 커지기 때문에 y축 값은 0으로 초기화
        return new Vector3(target.position.x, 0.0f, target.position.z);
    }

    protected virtual Vector3 targetPosSameYPos() // 플레이어 포지션 값
    {
        //플레이어 사이즈가 계속 커지기 때문에 y축 값은 0으로 초기화
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
            OnDestroyed.Invoke(); // 적이 파괴될 때 이벤트 발생
        }
    }
}
