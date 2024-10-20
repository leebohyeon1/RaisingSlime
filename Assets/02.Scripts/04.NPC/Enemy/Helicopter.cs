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
        richAI.updateRotation = true;
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

            // NavMeshAgent가 경로를 따라 이동하는 부분을 직접 제어
            if (distance > attackRange)
            {
                richAI.isStopped = false;

                MoveToTarget();

                AdjustAltitude();

                // 부드럽게 목표 지점을 향해 회전 (DoTween 사용)
                Quaternion targetRotation = Quaternion.LookRotation(TargetPosSameYPos() - transform.position);

                // X축 회전을 제한하는 로직 추가
                targetRotation = LimitXRotation(targetRotation, maxXAngle);

                transform.DORotateQuaternion(targetRotation, 1f);  // 1초 동안 회전
            }
            else
            {
                MaintainDistanceAndMove(distance);

                // 가까운 경우 부드럽게 회전
                Quaternion targetRotation = Quaternion.LookRotation(TargetGroundPos() - transform.position);

                // X축 회전을 제한하는 로직 추가
                targetRotation = LimitXRotation(targetRotation, maxXAngle);

                transform.DORotateQuaternion(targetRotation, 1f);  // 1초 동안 회전

                // 타겟이 공격 범위 내에 있을 때 총알 발사 시작
                Attack();
            }
        }
    }
    protected override void MoveToTarget()
    {
        if (target != null)
        {
            aiDestinationSetter.target = target;
        }
    }

    private void AdjustAltitude()
    {
        Vector3 nextPosition = richAI.steeringTarget;
        // Y 좌표는 공중에 있도록 flyHeight 값을 사용하여 수동 조작
        nextPosition.y = flyHeight;
        transform.position = Vector3.Lerp(transform.position, nextPosition, Time.deltaTime);
    }
    
    private void MaintainDistanceAndMove(float distanceToPlayer)
    {
        float desiredMinDistance = attackRange * 0.5f;
        float desiredMaxDistance = attackRange;

        if (distanceToPlayer < desiredMinDistance)
        {
            Vector3 directionAway = (transform.position - TargetPosSameYPos()).normalized;
            Vector3 movePosition = transform.position + directionAway * 3f;
            richAI.destination = movePosition;

            AdjustAltitude();
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
        foreach (var prop in propeller)
        {
            prop.transform.Rotate(Vector3.forward, propellerRotationSpeed * Time.deltaTime, Space.Self);
        }
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