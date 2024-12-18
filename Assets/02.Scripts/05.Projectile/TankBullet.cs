using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class TankBullet : Bullet
{
    private float gravity = 9.81f; // 하강할 때 적용될 가속도   
    private float explosionForce = 5f; // 폭발할 때 가해지는 힘
    private float explosionRadius = 5f; // 폭발 범위
    private float upwardsModifier = 1f; // 폭발 시 위로 튕겨나가는 힘의 비율
    private LayerMask explosionLayerMask; // 폭발의 영향을 받을 레이어 마스크 (플레이어, NPC 등)

    private Vector3 targetPos = Vector3.zero; // 타겟이 되는 플레이어 또는 목적지
    private Vector3 fallPos;
    private bool isFalling = false;

    [SerializeField]
    private GameObject explosionParticle;

    // Start is called before the first frame update
    protected override void Awake()
    {
        rb = GetComponent<Rigidbody>();

        lifeTimer = lifetime; // 수명 타이머 초기화
    }

    public override void OnUpdate(float dt)
    {
        if (!isFalling)
        {
            // float distanceToPlayer = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPos.x, 0, targetPos.z));

            float distanceSqrd = (new Vector3(transform.position.x, fallPos.y, transform.position.z) - fallPos).sqrMagnitude;
            // 플레이어와의 거리가 설정한 fallDistance보다 작아지면 하강 시작
            if (distanceSqrd <= 0.3f)
            {
                isFalling = true;
                StartFalling();
            }
        }

        base.OnUpdate(dt);

        transform.LookAt(transform.position + rb.velocity);
    }

    public void InitialTarget(Vector3 target, float force, float radius, float upModifier, LayerMask layer)
    {
        targetPos = target;
        explosionForce = force;
        explosionRadius = radius;
        upwardsModifier = upModifier;
        explosionLayerMask = layer;

        fallPos = Vector3.Lerp(transform.position, new Vector3(targetPos.x, 0, targetPos.z), 0.5f);
   
    }

    // 포탄이 떨어지도록 만드는 함수
    void StartFalling()
    {
        // 포탄의 Y축 속도에 중력 가속도를 추가하여 하강하게 만듦
        rb.useGravity = true;

        // 일정 속도 이상일 때 추가 중력 적용
        if (rb.velocity.magnitude > gravity)
        {
            // 속도가 높으면 중력을 더 많이 적용
            rb.velocity += Vector3.down * (rb.velocity.magnitude - gravity) * 1.5f;
        }
   
    }

    // 폭발 처리
    void Explode(Collider collision)
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
                //npc.TakeDamage(damage); // NPC에 데미지 적용
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

        explosionParticle.transform.SetParent(null);
        explosionParticle.SetActive(true);

        Vector3 collisionNormal = collision.transform.position - transform.position;
        explosionParticle.transform.rotation = Quaternion.LookRotation(-collisionNormal);

        Destroy(gameObject);

    }

    // 폭발 범위를 그리기 위한 디버그 용도
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        Gizmos.DrawLine(fallPos, Vector3.up);
    }

    protected override void OnTriggerEnter(Collider collision)
    {
        Explode(collision);
       
    }


}
