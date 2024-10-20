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
        richAI.updateRotation = true;
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

            // NavMeshAgent�� ��θ� ���� �̵��ϴ� �κ��� ���� ����
            if (distance > attackRange)
            {
                richAI.isStopped = false;

                MoveToTarget();

                AdjustAltitude();

                // �ε巴�� ��ǥ ������ ���� ȸ�� (DoTween ���)
                Quaternion targetRotation = Quaternion.LookRotation(TargetPosSameYPos() - transform.position);

                // X�� ȸ���� �����ϴ� ���� �߰�
                targetRotation = LimitXRotation(targetRotation, maxXAngle);

                transform.DORotateQuaternion(targetRotation, 1f);  // 1�� ���� ȸ��
            }
            else
            {
                MaintainDistanceAndMove(distance);

                // ����� ��� �ε巴�� ȸ��
                Quaternion targetRotation = Quaternion.LookRotation(TargetGroundPos() - transform.position);

                // X�� ȸ���� �����ϴ� ���� �߰�
                targetRotation = LimitXRotation(targetRotation, maxXAngle);

                transform.DORotateQuaternion(targetRotation, 1f);  // 1�� ���� ȸ��

                // Ÿ���� ���� ���� ���� ���� �� �Ѿ� �߻� ����
                Attack();
            }
        }
    }
    protected override void MoveToTarget()
    {
        if (target != null)
        {
            aiDestinationSetter.target = target;
        }
    }

    private void AdjustAltitude()
    {
        Vector3 nextPosition = richAI.steeringTarget;
        // Y ��ǥ�� ���߿� �ֵ��� flyHeight ���� ����Ͽ� ���� ����
        nextPosition.y = flyHeight;
        transform.position = Vector3.Lerp(transform.position, nextPosition, Time.deltaTime);
    }
    
    private void MaintainDistanceAndMove(float distanceToPlayer)
    {
        float desiredMinDistance = attackRange * 0.5f;
        float desiredMaxDistance = attackRange;

        if (distanceToPlayer < desiredMinDistance)
        {
            Vector3 directionAway = (transform.position - TargetPosSameYPos()).normalized;
            Vector3 movePosition = transform.position + directionAway * 3f;
            richAI.destination = movePosition;

            AdjustAltitude();
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
        foreach (var prop in propeller)
        {
            prop.transform.Rotate(Vector3.forward, propellerRotationSpeed * Time.deltaTime, Space.Self);
        }
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