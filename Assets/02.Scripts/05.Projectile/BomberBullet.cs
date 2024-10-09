using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BomberBullet : Bullet
{
    [TabGroup("포탄", "폭발"), LabelText("폭발 에너지"), SerializeField, Range(0.1f, 100f)]
    public float explosionForce = 5f; // 폭발할 때 가해지는 힘
    [TabGroup("포탄", "폭발"), LabelText("폭발 범위"), SerializeField, Range(0.1f, 100f)]
    public float explosionRadius = 5f; // 폭발 범위
    [TabGroup("포탄", "폭발"), LabelText("폭발 시 위로 튕기는 힘"), SerializeField, Range(0.1f, 20f)]
    public float upwardsModifier = 1f; // 폭발 시 위로 튕겨나가는 힘의 비율
    [TabGroup("포탄", "폭발"), LabelText("폭발에 영향받는 레이어"), SerializeField]
    public LayerMask explosionLayerMask; // 폭발의 영향을 받을 레이어 마스크 (플레이어, NPC 등)

    protected override void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lifeTimer = lifetime; // 수명 타이머 초기화
    }

    public void InitalBullet(float damage, float speed, float explosionForce, float explosionRadius, float upwardsModifier, LayerMask explosionLayerMask)
    {
        base.InitalBullet(damage, speed);

        this.explosionForce = explosionForce;
        this.explosionRadius = explosionRadius;
        this.upwardsModifier = upwardsModifier;
        this.explosionLayerMask = explosionLayerMask;

        rb.velocity = Vector3.down * this.speed;
    }

    // 폭발 처리
    void Explode()
    {
        // 폭발 효과를 처리할 범위를 지정 (폭발 반경 안의 모든 오브젝트 탐지)
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, explosionLayerMask);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            // NPC나 플레이어에게 추가적으로 데미지를 줄 수 있음
            NPCBase npc = hit.GetComponent<NPCBase>();
            if (npc != null)
            {
                npc.Explosion();
            }

            // Rigidbody가 있을 경우, 폭발력을 적용
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier, ForceMode.Impulse);
            }

            Player player = hit.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage); // 플레이어에 데미지 적용
            }
        }

        // 폭발 후 포탄을 제거
        Destroy(gameObject);
    }

    // 폭발 범위를 그리기 위한 디버그 용도
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    protected override void OnTriggerEnter(Collider collision)
    {
        Explode();
    }
}
