using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomber : NPCBase
{
    private Rigidbody rb;

    [TabGroup("���ݱ�", "����"), LabelText("���� ����"), SerializeField]
    private float spawnHeight;
    [TabGroup("���ݱ�", "����"), LabelText("���� ������"), SerializeField, Range(0.1f, 100f)]
    private float bulletDamage = 0.1f;
    [TabGroup("���ݱ�", "����"), LabelText("��ź �ӵ�"), SerializeField, Range(0.1f, 100f)]
    private float bulletSpeed = 0.1f;
    [TabGroup("���ݱ�", "����"), LabelText("���� ��ġ"), SerializeField]
    private Transform bombingTrans;

    [TabGroup("���ݱ�", "����"), LabelText("���� ������"), SerializeField, Range(0.1f, 100f)]
    public float explosionForce = 5f; // ������ �� �������� ��
    [TabGroup("���ݱ�", "����"), LabelText("���� ����"), SerializeField, Range(0.1f, 100f)]
    public float explosionRadius = 5f; // ���� ����
    [TabGroup("���ݱ�", "����"), LabelText("���� �� ���� ƨ��� ��"), SerializeField, Range(0.1f, 20f)]
    public float upwardsModifier = 1f; // ���� �� ���� ƨ�ܳ����� ���� ����
    [TabGroup("���ݱ�", "����"), LabelText("���߿� ����޴� ���̾�"), SerializeField]
    public LayerMask explosionLayerMask; // ������ ������ ���� ���̾� ����ũ (�÷��̾�, NPC ��)

    [LabelText("��ź"), SerializeField]
    private GameObject bulletPrefab;

    private Vector3 targetOnYPos; // Ÿ���� ������ ��ġ
    private bool isBombing = false;
    private bool isVisible = false;

    protected override void Awake()
    {
        rb = GetComponent<Rigidbody>();

        eatAbleObjectBase = GetComponent<EatAbleObjectBase>();
    }

    protected override void Start()
    {
        base.Start();

        // �ʱ� ���� ����
        transform.position = new Vector3(transform.position.x, spawnHeight, transform.position.z);

        // �̵� ���� �ʱ�ȭ
        targetOnYPos = TargetGroundPos();
        Vector3 velocity = targetOnYPos - new Vector3(transform.position.x, 0, transform.position.z); // ���ݱ��� �̵����� 
        velocity.y = 0; // y ��ġ 0���� �ʱ�ȭ

        // �̵�
        rb.velocity = velocity.normalized * moveSpeed;

        // �̵� �������� ��� �ٶ�
        transform.rotation = Quaternion.LookRotation(velocity);
        transform.Rotate(new Vector3(-90f, transform.rotation.y, 0));
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


            CheckPosition();
        }
    }

    // Ÿ�� ��ǥ�� �����̰��� ���� ����
    private void CheckPosition()
    {
        float distanceSqrd = (new Vector3(transform.position.x, 0, transform.position.z) - targetOnYPos).sqrMagnitude;

        if (!isBombing && distanceSqrd < 0.1f)
        {
            Bombing();
        }
    }

    // �����ϴ� �Լ�
    private void Bombing()
    {
        isBombing = true;

        GameObject bullet = Instantiate(bulletPrefab, bombingTrans.position, Quaternion.identity);

        bullet.GetComponent<BomberBullet>().InitalBullet(bulletDamage, bulletSpeed,
            explosionForce, explosionRadius, upwardsModifier, explosionLayerMask);

        bullet.GetComponent<Rigidbody>().velocity = Vector3.down * bulletSpeed;
    }

    private void OnBecameVisible()
    {
        isVisible = true;
    }

    private void OnBecameInvisible()
    {
        if (isVisible)
        {
            Destroy(gameObject);
        }
    }
}
