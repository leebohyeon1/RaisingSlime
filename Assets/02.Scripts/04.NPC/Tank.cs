using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : NPCBase
{
    [BoxGroup("탱크"), LabelText("공격 속도"), SerializeField, Range(0.1f, 20f)]
    private float attackSpeed = 2f;
    private float fireCooldown;

    [BoxGroup("탱크"), LabelText("공격 범위"), SerializeField, Range(1f, 20f)]
    private float attackRange = 10f;

    [BoxGroup("탱크"), LabelText("포탄 프리팹"), SerializeField]
    private GameObject bulletPrefab;

    [BoxGroup("탱크"), LabelText("총구 위치"), SerializeField]
    private Transform firePosistion;

    [BoxGroup("탱크"), LabelText("포탄 속도"), SerializeField, Range(1f, 20f)]
    private float bulletSpeed = 10f;

    [BoxGroup("탱크"), LabelText("포탄 데미지"), SerializeField, Range(0.1f, 20f)]
    private float bulletDamage = 1.0f;

    [BoxGroup("탱크"), LabelText("몸체 회전 속도"), SerializeField, Range(0.1f, 20f)]
    private float bodyRotationSpeed = 3f;  // 몸체 회전 속도 제한

    [BoxGroup("탱크"), LabelText("포신"), SerializeField]
    private Transform fireBarrel;

    [BoxGroup("탱크"), LabelText("포신 회전 속도"), SerializeField, Range(0.1f, 20f)]
    private float turretRotationSpeed = 5f;  // 포신 회전 속도 제한

    [BoxGroup("탱크"), LabelText("포신 최대 각도"), SerializeField, Range(0f, 90f)]
    private float maxTurretAngle = 45f;  // 포신의 최대 회전 각도

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void enemyAction()
    {
        if (eatAbleObjectBase.GetEaten())
        {
            return;
        }

        CheckNavMesh();

        float distanceToPlayer = Vector3.Distance(transform.position, TargetGroundPos());

        if (distanceToPlayer < attackRange)
        {
            // 타겟과 일정 거리를 유지하면서 움직임
            MaintainDistanceAndMove(distanceToPlayer);

            // 몸체 회전 및 공격 실행
            RotateBodyTowardsPlayer();  // 몸체 회전 추가
            Attack();
        }
        else if (distanceToPlayer >= attackRange)
        {
            agent.isStopped = false;
            agent.updateRotation = true;

            MoveToTarget();
        }
    }

    // 몸체 회전 메커니즘
    void RotateBodyTowardsPlayer()
    {
        agent.updateRotation = false;

        Vector3 directionToPlayer = TargetGroundPos() - transform.position; // 타겟과의 방향 계산
        directionToPlayer.y = 0; // 몸체는 수평 회전만 하므로 Y축 값은 무시

        if (directionToPlayer != Vector3.zero)
        {
            // 목표 회전값을 계산
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            // 몸체가 목표 회전으로 부드럽게 회전하도록 회전 속도를 제한
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, bodyRotationSpeed * Time.deltaTime);
        }
    }

    // 공격 메커니즘
    void Attack()
    {
        RotateTurretTowardsPlayer();  // 포신 회전 추가

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / attackSpeed;
        }
    }

    // 포신을 LookAt을 이용해 X축으로만 회전시켜 플레이어의 Y축 위치에 맞추도록 함
    void RotateTurretTowardsPlayer()
    {
        // 타겟의 위치를 계산
        Vector3 targetPosition = target.position;

        // LookAt을 사용하여 포신이 목표를 향해 회전하게 함
        firePosistion.LookAt(targetPosition);

        // Z축 기본값을 -90도로 유지하고, X축 회전값만 적용
        Vector3 currentEulerAngles = firePosistion.localEulerAngles;

        // X축만 회전하도록 설정, Y축 및 Z축은 고정 또는 제한
        float clampedX = Mathf.Clamp(currentEulerAngles.x, -maxTurretAngle, maxTurretAngle);

        // Z축은 -90도로 고정
        firePosistion.localEulerAngles = new Vector3(clampedX, firePosistion.localEulerAngles.y, -90f);
    }

    // 총알 발사fe
    void Shoot()
    {
        if (bulletPrefab != null && firePosistion != null)
        {
            GameObject bulletInstance = Instantiate(bulletPrefab, firePosistion.position, firePosistion.rotation);
            Bullet bulletScript = bulletInstance.GetComponent<Bullet>();

            if (bulletScript != null)
            {
                // 총알의 값을 터렛에서 설정한 값으로 변경
                bulletScript.damage = bulletDamage;
                bulletScript.speed = bulletSpeed;
            }
        }
    }

    // 타겟과 일정 거리를 유지하면서 움직이는 함수
    void MaintainDistanceAndMove(float distanceToPlayer)
    {
        // 원하는 최소 거리와 최대 거리
        float desiredMinDistance = attackRange * 0.7f;  // 최소 거리를 공격 범위의 70%으로 설정
        float desiredMaxDistance = attackRange;         // 최대 거리는 공격 범위

        // 현재 타겟과 너무 가깝다면 거리를 벌림
        if (distanceToPlayer < desiredMinDistance)
        {
            // 타겟 반대 방향으로 이동
            Vector3 directionAwayFromTarget = (transform.position - TargetGroundPos()).normalized;
            Vector3 movePosition = transform.position + directionAwayFromTarget * 1.5f; // 일정 거리 벌리기

            agent.SetDestination(movePosition);  // 타겟 반대 방향으로 이동
        }
    }
}
