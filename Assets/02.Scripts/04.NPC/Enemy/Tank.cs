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

    [TabGroup("��ũ", "����"), LabelText("���� ����"), SerializeField, Range(1f, 100f)]
    private float attackRange = 10f;

    [TabGroup("��ũ", "����"), LabelText("��ź ������"), SerializeField]
    private GameObject bulletPrefab;

    [TabGroup("��ũ", "����"), LabelText("��ź �ӵ�"), SerializeField, Range(1f, 20f)]
    private float bulletSpeed;

    [TabGroup("��ũ", "����"), LabelText("��ź ������"), SerializeField, Range(0.1f, 20f)]
    private float bulletDamage = 1.0f;

    [TabGroup("��ũ", "����"), LabelText("�ѱ� ��ġ"), SerializeField]
    private Transform[] firePosistion;

    [TabGroup("��ũ", "ȸ��"), LabelText("��ü ȸ�� �ӵ�"), SerializeField, Range(0.1f, 360f)]
    private float bodyRotationSpeed = 3f;  // ��ü ȸ�� �ӵ� ����

    [TabGroup("��ũ", "ȸ��"), LabelText("��ž ȸ�� �ӵ�"), SerializeField, Range(0.1f, 360f)]
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

                RotateTurretTowardsPlayer();  // ��ž ȸ�� �߰�

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
    // ��ü ȸ�� ��Ŀ���� (�ε巯�� ȸ�� ����)
    void RotateBodyTowardsPlayer()
    {
        aiPath.updateRotation = false;

        Vector3 directionToPlayer = aiPath.steeringTarget - transform.position; // Ÿ�ٰ��� ���� ���
     

        if (directionToPlayer != Vector3.zero)
        {
            // ��ǥ ȸ������ ���
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            // ��ü�� ��ǥ ȸ������ �ڿ������� ȸ���ϵ��� ȸ�� �ӵ��� ����
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, bodyRotationSpeed * Time.deltaTime);
        }
    }

    // ��ü ȸ�� ��Ŀ���� (�ε巯�� ȸ�� ����)
    void RotateTurretDefault()
    {
            turret.rotation = Quaternion.Slerp(transform.rotation, transform.rotation, bodyRotationSpeed * Time.deltaTime);
    
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
        if (target == null) return;

        // �÷��̾��� ��ġ�� ��ž�� ��ġ�� ����Ͽ� ��ǥ ������ ���
        Vector3 directionToPlayer = TargetGroundPos();
        Vector3 lookPosition = new Vector3(directionToPlayer.x, turret.position.y, directionToPlayer.z);

        // ��ǥ ȸ�� ���
        Quaternion targetRotation = Quaternion.LookRotation(lookPosition - turret.position);
        // ��ž�� ��ǥ ȸ������ �ڿ������� ȸ���ϵ��� ȸ�� �ӵ��� ����
        turret.rotation = Quaternion.Slerp(turret.rotation, targetRotation, turretRotationSpeed * Time.deltaTime);
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
            aiPath.isStopped = false;
            // Ÿ�� �ݴ� �������� �̵�
            Vector3 directionAwayFromTarget = (transform.position - TargetGroundPos()).normalized;
            Vector3 movePosition = transform.position + directionAwayFromTarget * 1.5f; // ���� �Ÿ� ������

            //aiDestinationSetter.target.position = movePosition;  // Ÿ�� �ݴ� �������� �̵�
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
            bulletScript.InitalBullet(bulletDamage, 0f);
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
            aiPath.isStopped = false;
            //aiPath.updateRotation = true;

        }
    }
}
