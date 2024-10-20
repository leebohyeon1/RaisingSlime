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

        richAI.updatePosition = false;
        richAI.updateRotation = false;

     
    }

    protected override void Start()
    {
        base.Start();

        // �︮���Ͱ� ������ �� �� ����
        Vector3 startPos = transform.position;
        startPos.y = flyHeight;
        transform.position = startPos;
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

            if (distance > attackRange)
            {
                richAI.isStopped = false;
                MoveToTarget();  // ��ǥ �������� �̵�
            }
            else
            {
                MaintainDistanceAndMove(distance);  // ���� ���� ������ ����
                Attack();  // ���� ����
            }

            // ���� �����ϸ� ��ǥ�� ȸ��
            Quaternion targetRotation = Quaternion.LookRotation(TargetPosSameYPos() - transform.position);
            targetRotation = LimitXRotation(targetRotation, maxXAngle);
            transform.DORotateQuaternion(targetRotation, 1f);  // 1�� ���� �ε巴�� ȸ��

            AdjustAltitude();  // �� ����
        }
    }

    protected override void MoveToTarget()
    {
        if (target != null)
        {
            richAI.destination = TargetPosSameYPos();  // ��ǥ ��ġ ����
        }
    }

    // �� ���� �Լ�: ��ǥ �������� �̵��� �� flyHeight�� ����
    private void AdjustAltitude()
    {
        Vector3 nextPosition = richAI.steeringTarget;
        nextPosition.y = flyHeight;  // �� ����
        transform.position = Vector3.Lerp(transform.position, nextPosition, Time.deltaTime * richAI.maxSpeed);
    }

    private void MaintainDistanceAndMove(float distanceToPlayer)
    {
        float desiredMinDistance = attackRange * 0.5f;
        float desiredMaxDistance = attackRange;

        if (distanceToPlayer < desiredMinDistance)
        {
            Vector3 directionAway = (transform.position - TargetPosSameYPos()).normalized;
            Vector3 movePosition = transform.position + directionAway * 3f;
            movePosition.y = flyHeight;  // �� ����
            richAI.destination = movePosition;
        }
        else
        {
            richAI.destination = transform.position;
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
        propeller[0].transform.Rotate(Vector3.forward, propellerRotationSpeed * Time.deltaTime, Space.Self);
        propeller[1].transform.Rotate(Vector3.up, propellerRotationSpeed * Time.deltaTime, Space.Self);
     
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