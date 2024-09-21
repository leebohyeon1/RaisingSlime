using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Police : NPCBase
{


    [BoxGroup("����"), LabelText("���� �ӵ�"), SerializeField, Range(0.1f, 20f)]
    private float attackSpeed = 2f;
    private float fireCooldown;

    [BoxGroup("����"), LabelText("���� ����"), SerializeField, Range(1f, 20f)]
    private float attackRange = 10f;

    [BoxGroup("����"), LabelText("�Ѿ� ������"), SerializeField]
    private GameObject bulletPrefab;

    [BoxGroup("����"), LabelText("�ѱ� ��ġ"), SerializeField]
    private Transform firePosistion;

    [BoxGroup("����"), LabelText("�Ѿ� �ӵ�"), SerializeField, Range(1f, 20f)]
    private float bulletSpeed = 10f;

    [BoxGroup("����"), LabelText("�Ѿ� ������"), SerializeField, Range(0.1f, 20f)]
    private float bulletDamage = 1.0f;

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

        float distanceToPlayer = Vector3.Distance(transform.position, targetGroundPos());

        if (distanceToPlayer < attackRange)
        {
            // Ÿ�ٰ� ���� �Ÿ��� �����ϸ鼭 ������
            MaintainDistanceAndMove(distanceToPlayer);

            // ���� ����
            Attack();
        }
        else if(distanceToPlayer >= attackRange) 
        {
            agent.isStopped = false;
            agent.SetDestination(targetGroundPos());
        }
    }

    // ���� ��Ŀ����
    void Attack()
    {
        transform.LookAt(targetPosSameYPos());

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / attackSpeed;
        }
    }

    // �Ѿ� �߻�
    void Shoot()
    {
        if (bulletPrefab != null && firePosistion != null)
        {
            GameObject bulletInstance = Instantiate(bulletPrefab, firePosistion.position, firePosistion.rotation);
            Bullet bulletScript = bulletInstance.GetComponent<Bullet>();

            if (bulletScript != null)
            {
                // �Ѿ��� ���� �ͷ����� ������ ������ ����
                bulletScript.damage = bulletDamage;
                bulletScript.speed = bulletSpeed;
            }
        }
    }

    // Ÿ�ٰ� ���� �Ÿ��� �����ϸ鼭 �����̴� �Լ�
    void MaintainDistanceAndMove(float distanceToPlayer)
    {
        // ���ϴ� �ּ� �Ÿ��� �ִ� �Ÿ�
        float desiredMinDistance = attackRange * 0.5f;  // �ּ� �Ÿ��� ���� ������ �������� ����
        float desiredMaxDistance = attackRange;         // �ִ� �Ÿ��� ���� ����

        // ���� Ÿ�ٰ� �ʹ� �����ٸ� �Ÿ��� ����
        if (distanceToPlayer < desiredMinDistance)
        {
            // Ÿ�� �ݴ� �������� �̵�
            Vector3 directionAwayFromTarget = (transform.position - targetGroundPos()).normalized;
            Vector3 movePosition = transform.position + directionAwayFromTarget * 1.5f; // ���� �Ÿ� ������

            agent.SetDestination(movePosition);  // Ÿ�� �ݴ� �������� �̵�
        }
    }
}
