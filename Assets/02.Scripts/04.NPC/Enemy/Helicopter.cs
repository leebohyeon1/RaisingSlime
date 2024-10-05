using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Helicopter : NPCBase
{
    [TabGroup("�︮����", "�̵�"), LabelText("���ƴٴϴ� ����"), SerializeField, Range(5f, 20f)]
    private float flyHeight = 10f;  // ���߿����� ����
    [TabGroup("�︮����", "�̵�"), LabelText("x�� �ִ� ȸ������"), SerializeField, Range(5f, 45f)]
    private float maxXAngle = 35f;  // x�� �ִ� ȸ������

    [TabGroup("�︮����", "����"), LabelText("���� ����"), SerializeField, Range(5f, 20f)]
    private float attackRange = 10f;

    [TabGroup("�︮����", "����"), LabelText("�߻� �ֱ� (��)"), SerializeField]
    private float shootInterval = 2f; // �� �ʸ��� �Ѿ��� �߻�����

    [TabGroup("�︮����", "����"), LabelText("�ѹ��� �߻��� �Ѿ� ����"), SerializeField]
    private int bulletsPerShot = 3; // �ѹ��� �߻��� �Ѿ� ����

    [TabGroup("�︮����", "����"), LabelText("���� �ӵ�"), SerializeField]
    private float attackSpeed = 2f; // �� �ʸ��� �Ѿ��� �߻�����
    private float fireCooldown = 0f;

    [TabGroup("�︮����", "����"), LabelText("�Ѿ� ������"), SerializeField]
    private GameObject bulletPrefab; // �Ѿ� ������

    [TabGroup("�︮����", "����"), LabelText("�Ѿ� ������"), SerializeField]
    private float bulletDamage; // �Ѿ� ������

    [TabGroup("�︮����", "����"), LabelText("�Ѿ� �ӵ�"), SerializeField]
    private float bulletSpeed; // �Ѿ� ������

    [TabGroup("�︮����", "����"), LabelText("�ѱ� ��ġ"), SerializeField]
    private Transform firePos;

    [TabGroup("�︮����", "���"), LabelText("�����緯"), SerializeField]
    private GameObject[] propeller;

    [TabGroup("�︮����", "���"), LabelText("�����緯 ȸ���ӵ�"), SerializeField]
    private float propellerRotationSpeed = 500f; // �����緯 ȸ�� �ӵ�

    private bool isShooting = false; // �Ѿ� �߻� ����

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        agent.updatePosition = false;  // NavMeshAgent�� Y ������ �ڵ� �̵����� �ʵ��� ����
        agent.updateRotation = true;   // ȸ���� ������ NavMeshAgent�� �ñ�
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;  // ȸ�� ��� ��Ȱ��ȭ
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void enemyAction()
    {
        RotatePropeller();

        if (eatAbleObjectBase.GetEaten())
        {
            return;
        }

        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, TargetPosSameYPos());

            // NavMeshAgent�� ��θ� ���� �̵��ϴ� �κ��� ���� ����
            if (distance > attackRange)
            {
                agent.isStopped = false;

                MoveToTarget();


                Vector3 nextPosition = agent.nextPosition;

                // Y ��ǥ�� ���߿� �ֵ��� flyHeight ���� ����Ͽ� ���� ����
                nextPosition.y = flyHeight;
                transform.position = nextPosition;

                // �ε巴�� ��ǥ ������ ���� ȸ�� (DoTween ���)
                //Quaternion targetRotation = Quaternion.LookRotation(TargetPosSameYPos() - transform.position);

                // X�� ȸ���� �����ϴ� ���� �߰�
               // targetRotation = LimitXRotation(targetRotation, maxXAngle);

               // transform.DORotateQuaternion(targetRotation, 1f);  // 1�� ���� ȸ��
            }
            else
            {
                MaintainDistanceAndMove(distance);

                // ����� ��� �ε巴�� ȸ��
                //Quaternion targetRotation = Quaternion.LookRotation(TargetGroundPos() - transform.position);

                // X�� ȸ���� �����ϴ� ���� �߰�
                // targetRotation = LimitXRotation(targetRotation, maxXAngle);

                //transform.DORotateQuaternion(targetRotation, 1f);  // 1�� ���� ȸ��

                // Ÿ���� ���� ���� ���� ���� �� �Ѿ� �߻� ����
                Attack();
            }
        }
    }

    // ���� ��Ŀ����
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

    IEnumerator Shoot()
    {
        isShooting = true;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            FireBullet();

            yield return new WaitForSeconds(shootInterval); // �߻� ����
        }

        isShooting = false; 
    }

    void FireBullet()
    {
        // �Ѿ��� �����ϰ� �߻� ���� ����
        if (bulletPrefab != null && target != null)
        {
            Vector3 direction = (target.position - firePos.position).normalized;
            GameObject bullet = Instantiate(bulletPrefab, firePos.position, Quaternion.LookRotation(direction));
          
            bullet.GetComponent<Bullet>().InitalBullet(bulletDamage, bulletSpeed);
        }
    }

    // X�� ȸ���� �����ϴ� �Լ�
    Quaternion LimitXRotation(Quaternion rotation, float maxXAngle)
    {
        // Euler ������ ��ȯ�Ͽ� ȸ���� ����
        Vector3 euler = rotation.eulerAngles;

        // X�� ȸ���� maxXAngle(��: 45��) �̻��� ��� ����
        if (euler.x > 180f)
        {
            euler.x -= 360f;  // ���� ������ ��ȯ�Ͽ� ó��
        }

        euler.x = Mathf.Clamp(euler.x, -maxXAngle, maxXAngle);

        return Quaternion.Euler(euler);  // ���ѵ� ȸ���� �ٽ� Quaternion���� ��ȯ
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
            Vector3 directionAwayFromTarget = (transform.position - TargetPosSameYPos()).normalized;
            Vector3 movePosition = transform.position + directionAwayFromTarget * 3f; // ���� �Ÿ� ������

            agent.SetDestination(movePosition);  // Ÿ�� �ݴ� �������� �̵�


            Vector3 nextPosition = agent.nextPosition;

            // Y ��ǥ�� ���߿� �ֵ��� flyHeight ���� ����Ͽ� ���� ����
            nextPosition.y = flyHeight; //+ Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;  // ���� ��鸲 �߰�
            transform.position = nextPosition;
        }
        else
        {
            agent.nextPosition = transform.position;
        }
    }

    void RotatePropeller() // �����緯 ȸ��
    {
        propeller[0].transform.Rotate(Vector3.forward, propellerRotationSpeed * Time.deltaTime, Space.Self);
        propeller[1].transform.Rotate(Vector3.up, propellerRotationSpeed * Time.deltaTime, Space.Self);
    }
}