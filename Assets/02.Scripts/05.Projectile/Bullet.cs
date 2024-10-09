using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IUpdateable
{
    [BoxGroup("Setting"), LabelText("�Ѿ� ������"), SerializeField]
    protected float damage = 5;
    [BoxGroup("Setting"), LabelText("�Ѿ� �ӵ�"), SerializeField]
    protected float speed = 20f;  // �Ѿ��� �⺻ �ӵ�
    [BoxGroup("Setting"), LabelText("�Ѿ� ����(��)"), SerializeField]
    protected float lifetime = 5f; // �Ѿ��� ������� �������� �ð�

    protected Rigidbody rb;       // Rigidbody ������Ʈ ����
    protected float lifeTimer;    // �Ѿ��� ���� Ÿ�̸�

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>(); // Rigidbody ������Ʈ ��������
        rb.velocity = transform.forward * speed; // �Ѿ��� �������� �߻�
        lifeTimer = lifetime; // ���� Ÿ�̸� �ʱ�ȭ
    }

    protected virtual void Start()
    {
        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    public virtual void OnUpdate(float dt)
    {
        // ������ ���ϸ� �Ѿ� �ı�
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // �� �±׷� ������ ������Ʈ���� ����
        {
            // ������ ���ظ� �ְų� �ٸ� ȿ���� �߰��� �� ����

            Player player = other.GetComponentInParent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

        }

        Destroy(gameObject); // �Ѿ� �ı�
    }

    public virtual void InitalBullet(float damage, float speed)
    {
        this.damage = damage;
        this.speed = speed;
    }

    public virtual void OnDestroy()
    {
        GameLogicManager.Instance.DeregisterUpdatableObject(this);
    }
}
