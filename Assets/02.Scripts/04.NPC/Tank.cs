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
    private Transform turret;

    [BoxGroup("탱크"), LabelText("포신 회전 속도"), SerializeField, Range(0.1f, 20f)]
    private float turretRotationSpeed = 5f;  // 포신 회전 속도 제한

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
            agent.isStopped = true;
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

    void RotateTurretTowardsPlayer()
    {
        // 현재 터렛의 회전 각도를 가져옴
        float currentAngle = turret.localEulerAngles.x;

        // 회전 각도를 -180도에서 180도 범위로 변환
        if (currentAngle > 180) currentAngle -= 360;

        // 회전 각도를 제한할 최소 및 최대 값 설정
        float minRotationAngle = -24f;  // 최소 각도 제한
        float maxRotationAngle = 3f;   // 최대 각도 제한

        // 타겟의 y 좌표에 따라 터렛을 회전시키는 로직
        if (turret.position.y > target.position.y)
        {
            // 회전하기 전에 각도 제한을 확인
            if (currentAngle < maxRotationAngle)
            {
                turret.Rotate(Vector3.right * turretRotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // 회전하기 전에 각도 제한을 확인
            if (currentAngle > minRotationAngle)
            {
                turret.Rotate(-Vector3.right * turretRotationSpeed * Time.deltaTime);
            }
        }

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
        float desiredMinDistance = attackRange * 0.4f;  // 최소 거리를 공격 범위의 70%으로 설정
        float desiredMaxDistance = attackRange;         // 최대 거리는 공격 범위

        // 현재 타겟과 너무 가깝다면 거리를 벌림
        if (distanceToPlayer < desiredMinDistance)
        {
            agent.isStopped = false;
            // 타겟 반대 방향으로 이동
            Vector3 directionAwayFromTarget = (transform.position - TargetGroundPos()).normalized;
            Vector3 movePosition = transform.position + directionAwayFromTarget * 1.5f; // 일정 거리 벌리기

            agent.SetDestination(movePosition);  // 타겟 반대 방향으로 이동
        }
    }
}
