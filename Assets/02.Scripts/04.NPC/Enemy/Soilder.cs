using DG.Tweening;
using HyperbitArsenal;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soilder : NPCBase
{


    [BoxGroup("군인"), LabelText("공격 속도"), SerializeField, Range(0.1f, 20f)]
    private float attackSpeed = 2f;
    private float fireCooldown;

    [BoxGroup("군인"), LabelText("공격 범위"), SerializeField, Range(1f, 20f)]
    private float attackRange = 10f;

    [BoxGroup("군인"), LabelText("총알 프리팹"), SerializeField]
    private GameObject bulletPrefab;

    [BoxGroup("군인"), LabelText("총구 위치"), SerializeField]
    private Transform firePosistion;

    [BoxGroup("군인"), LabelText("총알 속도"), SerializeField, Range(1f, 20f)]
    private float bulletSpeed = 10f;

    [BoxGroup("군인"), LabelText("총알 데미지"), SerializeField, Range(0.1f, 20f)]
    private float bulletDamage = 1.0f;

    [BoxGroup("군인"), LabelText("후퇴 위치"), SerializeField]
    private Transform backPos;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
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

            float distanceToPlayer = Vector3.Distance(transform.position, TargetGroundPos());

            if (distanceToPlayer < attackRange)
            {
                // 타겟과 일정 거리를 유지하면서 움직임
                MaintainDistanceAndMove(distanceToPlayer);

                // 공격 실행
                Attack();
            }
            else if (distanceToPlayer >= attackRange)
            {

                CheckDistanceToTarget();
                MoveToTarget();
            }
        }
     
    }

    // 공격 메커니즘
    void Attack()
    {
        transform.LookAt(TargetPosSameYPos());

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Shoot();

            AudioManager.Instance.PlaySFX("shoot1");

            fireCooldown = 1f / attackSpeed;
        }
    }

    // 총알 발사
    void Shoot()
    {
        if (bulletPrefab != null && firePosistion != null)
        {
            GameObject bulletInstance = Instantiate(bulletPrefab, firePosistion.position, firePosistion.rotation);
            Bullet bulletScript = bulletInstance.GetComponent<Bullet>();

            if (bulletScript != null)
            {
                bulletScript.InitalBullet(bulletDamage, bulletSpeed);
            }
        }
    }

    // 타겟과 일정 거리를 유지하면서 움직이는 함수
    void MaintainDistanceAndMove(float distanceToPlayer)
    {
        // 원하는 최소 거리와 최대 거리
        float desiredMinDistance = attackRange * 0.5f;  // 최소 거리를 공격 범위의 절반으로 설정
        float desiredMaxDistance = attackRange;         // 최대 거리는 공격 범위

        // 현재 타겟과 너무 가깝다면 거리를 벌림
        if (distanceToPlayer < desiredMinDistance)
        {
            Vector3 retreatDirection = (transform.position - TargetGroundPos()).normalized;
            retreatDirection.y = 0;  // Y축 이동 무시

            // 후퇴 방향으로 이동
            transform.position += retreatDirection * moveSpeed * Time.deltaTime;
        }
    }


}
