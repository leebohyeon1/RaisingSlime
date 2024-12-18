using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : NPCBase
{
    [TabGroup("탱크","공격"), LabelText("공격 속도"), SerializeField, Range(0.1f, 20f)]
    private float attackSpeed = 2f;
    private float fireCooldown;

    [TabGroup("탱크", "공격"), LabelText("공격 범위"), SerializeField, Range(1f, 100f)]
    private float attackRange = 10f;

    [TabGroup("탱크", "공격"), LabelText("포탄 프리팹"), SerializeField]
    private GameObject bulletPrefab;

    [TabGroup("탱크", "공격"), LabelText("포탄 속도"), SerializeField, Range(1f, 20f)]
    private float bulletSpeed;

    [TabGroup("탱크", "공격"), LabelText("포탄 데미지"), SerializeField, Range(0.1f, 20f)]
    private float bulletDamage = 1.0f;

    [TabGroup("탱크", "공격"), LabelText("총구 위치"), SerializeField]
    private Transform[] firePosistion;

    [TabGroup("탱크", "회전"), LabelText("몸체 회전 속도"), SerializeField, Range(0.1f, 360f)]
    private float bodyRotationSpeed = 3f;  // 몸체 회전 속도 제한

    [TabGroup("탱크", "회전"), LabelText("포탑 회전 속도"), SerializeField, Range(0.1f, 360f)]
    private float turretRotationSpeed = 5f;  // 포신 회전 속도 제한

    [TabGroup("탱크", "폭발"), LabelText("폭발 에너지"), SerializeField, Range(0.1f, 100f)]
    public float explosionForce = 5f; // 폭발할 때 가해지는 힘
    [TabGroup("탱크", "폭발"), LabelText("폭발 범위"), SerializeField, Range(0.1f, 100f)]
    public float explosionRadius = 5f; // 폭발 범위
    [TabGroup("탱크", "폭발"), LabelText("폭발 시 위로 튕기는 힘"), SerializeField, Range(0.1f, 20f)]
    public float upwardsModifier = 1f; // 폭발 시 위로 튕겨나가는 힘의 비율
    [TabGroup("탱크", "폭발"), LabelText("폭발에 영향받는 레이어"), SerializeField]
    public LayerMask explosionLayerMask; // 폭발의 영향을 받을 레이어 마스크 (플레이어, NPC 등)

    [TabGroup("탱크", "장식"), LabelText("포탑"), SerializeField]
    private Transform turret;

    [TabGroup("탱크", "장식"), LabelText("포신"), SerializeField]
    private Transform[] barrel;

    private int barrelIndex = 0;         // 포신 인덱스

    private Coroutine coroutine;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();



        AchievementManager.Instance.UpdateAchievement
            (AchievementManager.Instance.achievements[2].achievementName, 1);
    }

    protected override void enemyAction()
    {
        if (eatAbleObjectBase.GetEaten() || target == null)
        {
            aiPath.enabled = false;
            return;
        }
        else
        {
            aiPath.enabled = true;

            float distanceToPlayer = Vector3.Distance(transform.position, TargetGroundPos());

            if (distanceToPlayer < attackRange)
            {
                aiPath.isStopped = true;

                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }

                RotateTurretTowardsPlayer();  // 포탑 회전 추가

                Attack();
            }
            else if (distanceToPlayer >= attackRange)
            {
                coroutine = StartCoroutine(TankMoveOn());

                RotateTurretDefault();
                RotateBodyTowardsPlayer();
                CheckDistanceToTarget();
                MoveToTarget();
            }
        }

    


    }
    // 몸체 회전 메커니즘 (부드러운 회전 적용)
    void RotateBodyTowardsPlayer()
    {
        aiPath.updateRotation = false;

        Vector3 directionToPlayer = aiPath.steeringTarget - transform.position; // 타겟과의 방향 계산
     

        if (directionToPlayer != Vector3.zero)
        {
            // 목표 회전값을 계산
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            // 몸체가 목표 회전으로 자연스럽게 회전하도록 회전 속도를 제한
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, bodyRotationSpeed * Time.deltaTime);
        }
    }

    // 몸체 회전 메커니즘 (부드러운 회전 적용)
    void RotateTurretDefault()
    {
            turret.rotation = Quaternion.Slerp(transform.rotation, transform.rotation, bodyRotationSpeed * Time.deltaTime);
    
    }

    // 공격 메커니즘
    void Attack()
    {
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Shoot();
            AudioManager.Instance.PlaySFX("cluster_impact");
            fireCooldown = 1f / attackSpeed;
        }
    }
    void RotateTurretTowardsPlayer()
    {
        if (target == null) return;

        // 플레이어의 위치와 포탑의 위치를 사용하여 목표 방향을 계산
        Vector3 directionToPlayer = TargetGroundPos();
        Vector3 lookPosition = new Vector3(directionToPlayer.x, turret.position.y, directionToPlayer.z);

        // 목표 회전 계산
        Quaternion targetRotation = Quaternion.LookRotation(lookPosition - turret.position);
        // 포탑이 목표 회전으로 자연스럽게 회전하도록 회전 속도를 제한
        turret.rotation = Quaternion.Slerp(turret.rotation, targetRotation, turretRotationSpeed * Time.deltaTime);
    }



    // 총알 발사
    void Shoot()
    {
        if (bulletPrefab != null && firePosistion != null)
        {
            // 포신이 뒤로 갔다가 다시 원래 위치로 돌아오는 애니메이션 추가
            Transform currentBarrel = barrel[barrelIndex];
            Vector3 originalPosition = currentBarrel.localPosition;  // 원래 포신 위치 저장
            Vector3 recoilPosition = originalPosition - new Vector3(0.3f, 0, 0.05f);  // 뒤로 움직일 위치 설정

            // 포신이 뒤로 가는 애니메이션 (0.1초 동안)
            currentBarrel.DOLocalMove(recoilPosition, 0.2f).OnComplete(() =>
            {
                // 포신이 다시 원래 위치로 돌아오는 애니메이션 (0.2초 동안)
                currentBarrel.DOLocalMove(originalPosition, 0.9f / attackSpeed);
            });

            Fire();

            

            barrelIndex = (barrelIndex + 1) % 2;
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
            aiPath.isStopped = false;
            // 타겟 반대 방향으로 이동
            Vector3 directionAwayFromTarget = (transform.position - TargetGroundPos()).normalized;
            Vector3 movePosition = transform.position + directionAwayFromTarget * 1.5f; // 일정 거리 벌리기

            //aiDestinationSetter.target.position = movePosition;  // 타겟 반대 방향으로 이동
        }
    }

    void Fire()
    {  
        // 플레이어의 위치와 탱크의 수평 거리 계산
        Vector3 targetPosition = target.position;
        float distance = Vector3.Distance(new Vector3(firePosistion[barrelIndex].position.x, 0, firePosistion[barrelIndex].position.z), new Vector3(targetPosition.x, 0, targetPosition.z));
        
        // 포탄 생성 및 발사
        GameObject projectile = Instantiate(bulletPrefab, firePosistion[barrelIndex].position, firePosistion[barrelIndex].rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        TankBullet bulletScript = projectile.GetComponent<TankBullet>();

        // 총알의 데미지 설정
        if (bulletScript != null)
        {
            bulletScript.InitalBullet(bulletDamage, 0f);
            bulletScript.InitialTarget(new Vector3(firePosistion[barrelIndex].position.x, 0, firePosistion[barrelIndex].position.z) + (firePosistion[barrelIndex].forward * distance)
                ,explosionForce,explosionRadius,upwardsModifier,explosionLayerMask);
        }

        // 발사 방향 설정 (일단 직선으로 발사)
        Vector3 launchDirection = firePosistion[barrelIndex].forward;
        rb.velocity = launchDirection * bulletSpeed;

    }

    IEnumerator TankMoveOn()
    {
        yield return new WaitForSeconds(2f);

        float distanceToPlayer = Vector3.Distance(transform.position, TargetGroundPos());

        if (distanceToPlayer >= attackRange)
        {
            aiPath.isStopped = false;
            //aiPath.updateRotation = true;

        }
    }
}
