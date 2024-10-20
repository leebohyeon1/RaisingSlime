using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Helicopter : NPCBase
{
    [TabGroup("헬리콥터", "이동"), LabelText("날아다니는 높이"), SerializeField, Range(5f, 20f)]
    private float flyHeight = 10f;  // 공중에서의 높이
    
    [TabGroup("헬리콥터", "이동"), LabelText("x축 최대 회전각도"), SerializeField, Range(5f, 45f)]
    private float maxXAngle = 35f;  // x축 최대 회전각도

    [TabGroup("헬리콥터", "공격"), LabelText("공격 범위"), SerializeField, Range(5f, 20f)]
    private float attackRange = 10f;

    [TabGroup("헬리콥터", "공격"), LabelText("발사 주기 (초)"), SerializeField]
    private float shootInterval = 2f; // 몇 초마다 총알을 발사할지

    [TabGroup("헬리콥터", "공격"), LabelText("한번에 발사할 총알 개수"), SerializeField]
    private int bulletsPerShot = 3; // 한번에 발사할 총알 개수

    [TabGroup("헬리콥터", "공격"), LabelText("공격 속도"), SerializeField]
    private float attackSpeed = 2f; // 몇 초마다 총알을 발사할지
    private float fireCooldown = 0f;

    [TabGroup("헬리콥터", "공격"), LabelText("총알 프리팹"), SerializeField]
    private GameObject bulletPrefab; // 총알 프리팹

    [TabGroup("헬리콥터", "공격"), LabelText("총알 데미지"), SerializeField]
    private float bulletDamage; // 총알 데미지

    [TabGroup("헬리콥터", "공격"), LabelText("총알 속도"), SerializeField]
    private float bulletSpeed; // 총알 데미지

    [TabGroup("헬리콥터", "공격"), LabelText("총구 위치"), SerializeField]
    private Transform firePos;

    [TabGroup("헬리콥터", "장식"), LabelText("프로펠러"), SerializeField]
    private GameObject[] propeller;

    [TabGroup("헬리콥터", "장식"), LabelText("프로펠러 회전속도"), SerializeField]
    private float propellerRotationSpeed = 500f; // 프로펠러 회전 속도

    private bool isShooting = false; // 총알 발사 여부

    protected override void Awake()
    {
        base.Awake();

        richAI.updatePosition = false;
        richAI.updateRotation = false;

     
    }

    protected override void Start()
    {
        base.Start();

        // 헬리콥터가 시작할 때 고도 조정
        Vector3 startPos = transform.position;
        startPos.y = flyHeight;
        transform.position = startPos;
    }

    protected override void enemyAction()
    {
        RotatePropeller();

        if (eatAbleObjectBase.GetEaten() || target == null)
        {
            richAI.enabled = false;
            return;
        }
        else
        {
            richAI.enabled = true;

            float distance = Vector3.Distance(transform.position, TargetPosSameYPos());

            if (distance > attackRange)
            {
                richAI.isStopped = false;
                MoveToTarget();  // 목표 지점으로 이동
            }
            else
            {
                MaintainDistanceAndMove(distance);  // 공격 범위 내에서 유지
                Attack();  // 공격 시작
            }

            // 고도를 유지하며 목표로 회전
            Quaternion targetRotation = Quaternion.LookRotation(TargetPosSameYPos() - transform.position);
            targetRotation = LimitXRotation(targetRotation, maxXAngle);
            transform.DORotateQuaternion(targetRotation, 1f);  // 1초 동안 부드럽게 회전

            AdjustAltitude();  // 고도 조정
        }
    }

    protected override void MoveToTarget()
    {
        if (target != null)
        {
            richAI.destination = TargetPosSameYPos();  // 목표 위치 설정
        }
    }

    // 고도 유지 함수: 목표 지점으로 이동할 때 flyHeight를 유지
    private void AdjustAltitude()
    {
        Vector3 nextPosition = richAI.steeringTarget;
        nextPosition.y = flyHeight;  // 고도 유지
        transform.position = Vector3.Lerp(transform.position, nextPosition, Time.deltaTime * richAI.maxSpeed);
    }

    private void MaintainDistanceAndMove(float distanceToPlayer)
    {
        float desiredMinDistance = attackRange * 0.5f;
        float desiredMaxDistance = attackRange;

        if (distanceToPlayer < desiredMinDistance)
        {
            Vector3 directionAway = (transform.position - TargetPosSameYPos()).normalized;
            Vector3 movePosition = transform.position + directionAway * 3f;
            movePosition.y = flyHeight;  // 고도 유지
            richAI.destination = movePosition;
        }
        else
        {
            richAI.destination = transform.position;
        }
    }


    // 공격 메커니즘
    void Attack()
    {
        //transform.LookAt(TargetPosSameYPos());

        if (!isShooting)
        {
            fireCooldown -= Time.deltaTime;
        }
            
        if (fireCooldown <= 0f)
        {
            StartCoroutine(Shoot());
            fireCooldown = 1f / attackSpeed;
        }
    }

    private IEnumerator Shoot()
    {
        isShooting = true;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            FireBullet();
            yield return new WaitForSeconds(shootInterval);
        }

        isShooting = false;
    }

    private void FireBullet()
    {
        if (bulletPrefab != null && target != null)
        {
            Vector3 direction = (target.position - firePos.position).normalized;
            GameObject bullet = Instantiate(bulletPrefab, firePos.position, Quaternion.LookRotation(direction));
            bullet.GetComponent<Bullet>().InitalBullet(bulletDamage, bulletSpeed);
        }
    }

    private void RotatePropeller()
    {
        propeller[0].transform.Rotate(Vector3.forward, propellerRotationSpeed * Time.deltaTime, Space.Self);
        propeller[1].transform.Rotate(Vector3.up, propellerRotationSpeed * Time.deltaTime, Space.Self);
     
    }

    private Quaternion LimitXRotation(Quaternion rotation, float maxXAngle)
    {
        Vector3 euler = rotation.eulerAngles;

        if (euler.x > 180f)
        {
            euler.x -= 360f;
        }

        euler.x = Mathf.Clamp(euler.x, -maxXAngle, maxXAngle);

        return Quaternion.Euler(euler);
    }
}