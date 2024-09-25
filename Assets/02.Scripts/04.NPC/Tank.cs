using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : NPCBase
{
    [BoxGroup("��ũ"), LabelText("���� �ӵ�"), SerializeField, Range(0.1f, 20f)]
    private float attackSpeed = 2f;
    private float fireCooldown;

    [BoxGroup("��ũ"), LabelText("���� ����"), SerializeField, Range(1f, 20f)]
    private float attackRange = 10f;

    [BoxGroup("��ũ"), LabelText("��ź ������"), SerializeField]
    private GameObject bulletPrefab;

    [BoxGroup("��ũ"), LabelText("�ѱ� ��ġ"), SerializeField]
    private Transform firePosistion;

    [BoxGroup("��ũ"), LabelText("��ź �ӵ�"), SerializeField, Range(1f, 20f)]
    private float bulletSpeed = 10f;

    [BoxGroup("��ũ"), LabelText("��ź ������"), SerializeField, Range(0.1f, 20f)]
    private float bulletDamage = 1.0f;

    [BoxGroup("��ũ"), LabelText("��ü ȸ�� �ӵ�"), SerializeField, Range(0.1f, 20f)]
    private float bodyRotationSpeed = 3f;  // ��ü ȸ�� �ӵ� ����

    [BoxGroup("��ũ"), LabelText("����"), SerializeField]
    private Transform fireBarrel;

    [BoxGroup("��ũ"), LabelText("���� ȸ�� �ӵ�"), SerializeField, Range(0.1f, 20f)]
    private float turretRotationSpeed = 5f;  // ���� ȸ�� �ӵ� ����

    [BoxGroup("��ũ"), LabelText("���� �ִ� ����"), SerializeField, Range(0f, 90f)]
    private float maxTurretAngle = 45f;  // ������ �ִ� ȸ�� ����

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
            // Ÿ�ٰ� ���� �Ÿ��� �����ϸ鼭 ������
            MaintainDistanceAndMove(distanceToPlayer);

            // ��ü ȸ�� �� ���� ����
            RotateBodyTowardsPlayer();  // ��ü ȸ�� �߰�
            Attack();
        }
        else if (distanceToPlayer >= attackRange)
        {
            agent.isStopped = false;
            agent.updateRotation = true;

            MoveToTarget();
        }
    }

    // ��ü ȸ�� ��Ŀ����
    void RotateBodyTowardsPlayer()
    {
        agent.updateRotation = false;

        Vector3 directionToPlayer = TargetGroundPos() - transform.position; // Ÿ�ٰ��� ���� ���
        directionToPlayer.y = 0; // ��ü�� ���� ȸ���� �ϹǷ� Y�� ���� ����

        if (directionToPlayer != Vector3.zero)
        {
            // ��ǥ ȸ������ ���
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            // ��ü�� ��ǥ ȸ������ �ε巴�� ȸ���ϵ��� ȸ�� �ӵ��� ����
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, bodyRotationSpeed * Time.deltaTime);
        }
    }

    // ���� ��Ŀ����
    void Attack()
    {
        RotateTurretTowardsPlayer();  // ���� ȸ�� �߰�

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / attackSpeed;
        }
    }

    // ������ LookAt�� �̿��� X�����θ� ȸ������ �÷��̾��� Y�� ��ġ�� ���ߵ��� ��
    void RotateTurretTowardsPlayer()
    {
        // Ÿ���� ��ġ�� ���
        Vector3 targetPosition = target.position;

        // LookAt�� ����Ͽ� ������ ��ǥ�� ���� ȸ���ϰ� ��
        firePosistion.LookAt(targetPosition);

        // Z�� �⺻���� -90���� �����ϰ�, X�� ȸ������ ����
        Vector3 currentEulerAngles = firePosistion.localEulerAngles;

        // X�ุ ȸ���ϵ��� ����, Y�� �� Z���� ���� �Ǵ� ����
        float clampedX = Mathf.Clamp(currentEulerAngles.x, -maxTurretAngle, maxTurretAngle);

        // Z���� -90���� ����
        firePosistion.localEulerAngles = new Vector3(clampedX, firePosistion.localEulerAngles.y, -90f);
    }

    // �Ѿ� �߻�fe
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
        float desiredMinDistance = attackRange * 0.7f;  // �ּ� �Ÿ��� ���� ������ 70%���� ����
        float desiredMaxDistance = attackRange;         // �ִ� �Ÿ��� ���� ����

        // ���� Ÿ�ٰ� �ʹ� �����ٸ� �Ÿ��� ����
        if (distanceToPlayer < desiredMinDistance)
        {
            // Ÿ�� �ݴ� �������� �̵�
            Vector3 directionAwayFromTarget = (transform.position - TargetGroundPos()).normalized;
            Vector3 movePosition = transform.position + directionAwayFromTarget * 1.5f; // ���� �Ÿ� ������

            agent.SetDestination(movePosition);  // Ÿ�� �ݴ� �������� �̵�
        }
    }
}
