using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomber : NPCBase
{
    private Rigidbody rb;

    [TabGroup("폭격기", "스폰"), LabelText("스폰 높이"), SerializeField]
    private float spawnHeight;
    [TabGroup("폭격기", "공격"), LabelText("폭격 데미지"), SerializeField, Range(0.1f, 100f)]
    private float bulletDamage = 0.1f;
    [TabGroup("폭격기", "공격"), LabelText("포탄 속도"), SerializeField, Range(0.1f, 100f)]
    private float bulletSpeed = 0.1f;
    [TabGroup("폭격기", "공격"), LabelText("폭격 위치"), SerializeField]
    private Transform bombingTrans;

    [TabGroup("폭격기", "폭발"), LabelText("폭발 에너지"), SerializeField, Range(0.1f, 100f)]
    public float explosionForce = 5f; // 폭발할 때 가해지는 힘
    [TabGroup("폭격기", "폭발"), LabelText("폭발 범위"), SerializeField, Range(0.1f, 100f)]
    public float explosionRadius = 5f; // 폭발 범위
    [TabGroup("폭격기", "폭발"), LabelText("폭발 시 위로 튕기는 힘"), SerializeField, Range(0.1f, 20f)]
    public float upwardsModifier = 1f; // 폭발 시 위로 튕겨나가는 힘의 비율
    [TabGroup("폭격기", "폭발"), LabelText("폭발에 영향받는 레이어"), SerializeField]
    public LayerMask explosionLayerMask; // 폭발의 영향을 받을 레이어 마스크 (플레이어, NPC 등)

    [LabelText("폭탄"), SerializeField]
    private GameObject bulletPrefab;

    private Vector3 targetOnYPos; // 타겟의 땅위의 위치
    private bool isBombing = false;
    private bool isVisible = false;

    protected override void Awake()
    {
        rb = GetComponent<Rigidbody>();

        eatAbleObjectBase = GetComponent<EatAbleObjectBase>();
    }

    protected override void Start()
    {
        base.Start();

        // 초기 높이 설정
        transform.position = new Vector3(transform.position.x, spawnHeight, transform.position.z);

        // 이동 방향 초기화
        targetOnYPos = TargetGroundPos();
        Vector3 velocity = targetOnYPos - new Vector3(transform.position.x, 0, transform.position.z); // 폭격기의 이동방향 
        velocity.y = 0; // y 위치 0으로 초기화

        // 이동
        rb.velocity = velocity.normalized * moveSpeed;

        // 이동 방향으로 즉시 바라봄
        transform.rotation = Quaternion.LookRotation(velocity);
        transform.Rotate(new Vector3(-90f, transform.rotation.y, 0));
    }

    protected override void enemyAction()
    {

        if (eatAbleObjectBase.GetEaten() || target == null || isExplosion)
        {
            aiPath.enabled = false;
            return;
        }
        else
        {
            aiPath.enabled = true;


            CheckPosition();
        }
    }

    // 타겟 목표에 가까이가면 폭격 시작
    private void CheckPosition()
    {
        float distanceSqrd = (new Vector3(transform.position.x, 0, transform.position.z) - targetOnYPos).sqrMagnitude;

        if (!isBombing && distanceSqrd < 0.1f)
        {
            Bombing();
        }
    }

    // 폭격하는 함수
    private void Bombing()
    {
        isBombing = true;

        GameObject bullet = Instantiate(bulletPrefab, bombingTrans.position, Quaternion.identity);

        bullet.GetComponent<BomberBullet>().InitalBullet(bulletDamage, bulletSpeed,
            explosionForce, explosionRadius, upwardsModifier, explosionLayerMask);

        bullet.GetComponent<Rigidbody>().velocity = Vector3.down * bulletSpeed;
    }

    private void OnBecameVisible()
    {
        isVisible = true;
    }

    private void OnBecameInvisible()
    {
        if (isVisible)
        {
            Destroy(gameObject);
        }
    }
}
