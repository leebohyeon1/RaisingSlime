using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : NPCBase
{
    [TabGroup("��ũ","����"), LabelText("���� �ӵ�"), SerializeField, Range(0.1f, 20f)]
    private float attackSpeed = 2f;
    private float fireCooldown;

    [TabGroup("��ũ", "����"), LabelText("���� ����"), SerializeField, Range(1f, 20f)]
    private float attackRange = 10f;

    [TabGroup("��ũ", "����"), LabelText("��ź ������"), SerializeField]
    private GameObject bulletPrefab;

    [TabGroup("��ũ", "����"), LabelText("��ź �ӵ�"), SerializeField, Range(1f, 20f)]
    private float bulletSpeed;

    [TabGroup("��ũ", "����"), LabelText("��ź ������"), SerializeField, Range(0.1f, 20f)]
    private float bulletDamage = 1.0f;

    [TabGroup("��ũ", "����"), LabelText("�ѱ� ��ġ"), SerializeField]
    private Transform[] firePosistion;

    [TabGroup("��ũ", "ȸ��"), LabelText("��ü ȸ�� �ӵ�"), SerializeField, Range(0.1f, 20f)]
    private float bodyRotationSpeed = 3f;  // ��ü ȸ�� �ӵ� ����

    [TabGroup("��ũ", "ȸ��"), LabelText("��ž ȸ�� �ӵ�"), SerializeField, Range(0.1f, 40f)]
    private float turretRotationSpeed = 5f;  // ���� ȸ�� �ӵ� ����

    [TabGroup("��ũ", "����"), LabelText("���� ������"), SerializeField, Range(0.1f, 100f)]
    public float explosionForce = 5f; // ������ �� �������� ��
    [TabGroup("��ũ", "����"), LabelText("���� ����"), SerializeField, Range(0.1f, 100f)]
    public float explosionRadius = 5f; // ���� ����
    [TabGroup("��ũ", "����"), LabelText("���� �� ���� ƨ��� ��"), SerializeField, Range(0.1f, 20f)]
    public float upwardsModifier = 1f; // ���� �� ���� ƨ�ܳ����� ���� ����
    [TabGroup("��ũ", "����"), LabelText("���߿� ����޴� ���̾�"), SerializeField]
    public LayerMask explosionLayerMask; // ������ ������ ���� ���̾� ����ũ (�÷��̾�, NPC ��)

    [TabGroup("��ũ", "���"), LabelText("��ž"), SerializeField]
    private Transform turret;

    [TabGroup("��ũ", "���"), LabelText("����"), SerializeField]
    private Transform[] barrel;

    private int barrelIndex = 0;         // ���� �ε���

    private Coroutine coroutine;

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
        RotateTurretTowardsPlayer();  // ��ž ȸ�� �߰�

        float distanceToPlayer = Vector3.Distance(transform.position, TargetGroundPos());

        if (distanceToPlayer < attackRange)
        {
            agent.isStopped = true;

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            Attack();
        }
        else if (distanceToPlayer >= attackRange)
        {
            coroutine = StartCoroutine(TankMoveOn());

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
     

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / attackSpeed;
        }
    }

    void RotateTurretTowardsPlayer()
    {
        // �÷��̾��� �̵� ������ ����� ��ǥ ��ġ ���
        Rigidbody playerRb = target.GetComponent<Rigidbody>();  // �ߺ� ȣ�� ����
        
        Vector3 directionToPlayer = TargetGroundPos() + (new Vector3(playerRb.velocity.x,0,playerRb.velocity.z));

        // Y�� ȸ���� �����ϱ� ���� ���� ���� �ͷ��� ���̷� ����
        Vector3 lookPosition = new Vector3(directionToPlayer.x, turret.position.y, directionToPlayer.z);

        // �÷��̾ �ٶ󺸴� ��ǥ ȸ�� �� ��� �� �ε巴�� ȸ��
        Quaternion targetRotation = Quaternion.LookRotation(lookPosition - turret.position);
        turret.rotation = Quaternion.RotateTowards(turret.rotation, targetRotation, turretRotationSpeed * Time.deltaTime);
    }


    // �Ѿ� �߻�
    void Shoot()
    {
        if (bulletPrefab != null && firePosistion != null)
        {
            // ������ �ڷ� ���ٰ� �ٽ� ���� ��ġ�� ���ƿ��� �ִϸ��̼� �߰�
            Transform currentBarrel = barrel[barrelIndex];
            Vector3 originalPosition = currentBarrel.localPosition;  // ���� ���� ��ġ ����
            Vector3 recoilPosition = originalPosition - new Vector3(0.3f, 0, 0.05f);  // �ڷ� ������ ��ġ ����

            // ������ �ڷ� ���� �ִϸ��̼� (0.1�� ����)
            currentBarrel.DOLocalMove(recoilPosition, 0.2f).OnComplete(() =>
            {
                // ������ �ٽ� ���� ��ġ�� ���ƿ��� �ִϸ��̼� (0.2�� ����)
                currentBarrel.DOLocalMove(originalPosition, 0.9f / attackSpeed);
            });

            Fire();

            

            barrelIndex = (barrelIndex + 1) % 2;
        }
    }

    // Ÿ�ٰ� ���� �Ÿ��� �����ϸ鼭 �����̴� �Լ�
    void MaintainDistanceAndMove(float distanceToPlayer)
    {
        // ���ϴ� �ּ� �Ÿ��� �ִ� �Ÿ�
        float desiredMinDistance = attackRange * 0.4f;  // �ּ� �Ÿ��� ���� ������ 70%���� ����
        float desiredMaxDistance = attackRange;         // �ִ� �Ÿ��� ���� ����

        // ���� Ÿ�ٰ� �ʹ� �����ٸ� �Ÿ��� ����
        if (distanceToPlayer < desiredMinDistance)
        {
            agent.isStopped = false;
            // Ÿ�� �ݴ� �������� �̵�
            Vector3 directionAwayFromTarget = (transform.position - TargetGroundPos()).normalized;
            Vector3 movePosition = transform.position + directionAwayFromTarget * 1.5f; // ���� �Ÿ� ������

            agent.SetDestination(movePosition);  // Ÿ�� �ݴ� �������� �̵�
        }
    }
    void Fire()
    {  
        // �÷��̾��� ��ġ�� ��ũ�� ���� �Ÿ� ���
        Vector3 targetPosition = target.position;
        float distance = Vector3.Distance(new Vector3(firePosistion[barrelIndex].position.x, 0, firePosistion[barrelIndex].position.z), new Vector3(targetPosition.x, 0, targetPosition.z));
        
        // ��ź ���� �� �߻�
        GameObject projectile = Instantiate(bulletPrefab, firePosistion[barrelIndex].position, firePosistion[barrelIndex].rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        TankBullet bulletScript = projectile.GetComponent<TankBullet>();

        // �Ѿ��� ������ ����
        if (bulletScript != null)
        {
            bulletScript.damage = bulletDamage;
            bulletScript.InitialTarget(new Vector3(firePosistion[barrelIndex].position.x, 0, firePosistion[barrelIndex].position.z) + (firePosistion[barrelIndex].forward * distance)
                ,explosionForce,explosionRadius,upwardsModifier,explosionLayerMask);
        }

        // �߻� ���� ���� (�ϴ� �������� �߻�)
        Vector3 launchDirection = firePosistion[barrelIndex].forward;
        rb.velocity = launchDirection * bulletSpeed;

    }

    IEnumerator TankMoveOn()
    {
        yield return new WaitForSeconds(2f);

        float distanceToPlayer = Vector3.Distance(transform.position, TargetGroundPos());

        if (distanceToPlayer >= attackRange)
        {
            agent.isStopped = false;
            agent.updateRotation = true;

        }
    }
}
