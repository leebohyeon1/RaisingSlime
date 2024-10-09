using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BomberBullet : Bullet
{
    [TabGroup("��ź", "����"), LabelText("���� ������"), SerializeField, Range(0.1f, 100f)]
    public float explosionForce = 5f; // ������ �� �������� ��
    [TabGroup("��ź", "����"), LabelText("���� ����"), SerializeField, Range(0.1f, 100f)]
    public float explosionRadius = 5f; // ���� ����
    [TabGroup("��ź", "����"), LabelText("���� �� ���� ƨ��� ��"), SerializeField, Range(0.1f, 20f)]
    public float upwardsModifier = 1f; // ���� �� ���� ƨ�ܳ����� ���� ����
    [TabGroup("��ź", "����"), LabelText("���߿� ����޴� ���̾�"), SerializeField]
    public LayerMask explosionLayerMask; // ������ ������ ���� ���̾� ����ũ (�÷��̾�, NPC ��)

    protected override void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lifeTimer = lifetime; // ���� Ÿ�̸� �ʱ�ȭ
    }

    public void InitalBullet(float damage, float speed, float explosionForce, float explosionRadius, float upwardsModifier, LayerMask explosionLayerMask)
    {
        base.InitalBullet(damage, speed);

        this.explosionForce = explosionForce;
        this.explosionRadius = explosionRadius;
        this.upwardsModifier = upwardsModifier;
        this.explosionLayerMask = explosionLayerMask;

        rb.velocity = Vector3.down * this.speed;
    }

    // ���� ó��
    void Explode()
    {
        // ���� ȿ���� ó���� ������ ���� (���� �ݰ� ���� ��� ������Ʈ Ž��)
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, explosionLayerMask);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            // NPC�� �÷��̾�� �߰������� �������� �� �� ����
            NPCBase npc = hit.GetComponent<NPCBase>();
            if (npc != null)
            {
                npc.Explosion();
            }

            // Rigidbody�� ���� ���, ���߷��� ����
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier, ForceMode.Impulse);
            }

            Player player = hit.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage); // �÷��̾ ������ ����
            }
        }

        // ���� �� ��ź�� ����
        Destroy(gameObject);
    }

    // ���� ������ �׸��� ���� ����� �뵵
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    protected override void OnTriggerEnter(Collider collision)
    {
        Explode();
    }
}
