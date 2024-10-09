using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IUpdateable
{
    [BoxGroup("Setting"), LabelText("총알 데미지"), SerializeField]
    protected float damage = 5;
    [BoxGroup("Setting"), LabelText("총알 속도"), SerializeField]
    protected float speed = 20f;  // 총알의 기본 속도
    [BoxGroup("Setting"), LabelText("총알 수명(초)"), SerializeField]
    protected float lifetime = 5f; // 총알이 사라지기 전까지의 시간

    protected Rigidbody rb;       // Rigidbody 컴포넌트 참조
    protected float lifeTimer;    // 총알의 수명 타이머

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>(); // Rigidbody 컴포넌트 가져오기
        rb.velocity = transform.forward * speed; // 총알을 전방으로 발사
        lifeTimer = lifetime; // 수명 타이머 초기화
    }

    protected virtual void Start()
    {
        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    public virtual void OnUpdate(float dt)
    {
        // 수명이 다하면 총알 파괴
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // 적 태그로 설정된 오브젝트에만 반응
        {
            // 적에게 피해를 주거나 다른 효과를 추가할 수 있음

            Player player = other.GetComponentInParent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

        }

        Destroy(gameObject); // 총알 파괴
    }

    public virtual void InitalBullet(float damage, float speed)
    {
        this.damage = damage;
        this.speed = speed;
    }

    public virtual void OnDestroy()
    {
        GameLogicManager.Instance.DeregisterUpdatableObject(this);
    }
}
