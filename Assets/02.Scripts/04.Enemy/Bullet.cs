using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [BoxGroup("Setting"), LabelText("�Ѿ� ������")]
    public float damage = 5;
    [BoxGroup("Setting"), LabelText("�Ѿ� �ӵ�")]
    public float speed = 20f;  // �Ѿ��� �⺻ �ӵ�
    [BoxGroup("Setting"), LabelText("�Ѿ� ����(��)")]
    public float lifetime = 5f; // �Ѿ��� ������� �������� �ð�

    private Rigidbody rb;       // Rigidbody ������Ʈ ����
    private float lifeTimer;    // �Ѿ��� ���� Ÿ�̸�

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Rigidbody ������Ʈ ��������
        rb.velocity = transform.forward * speed; // �Ѿ��� �������� �߻�
        lifeTimer = lifetime; // ���� Ÿ�̸� �ʱ�ȭ
    }

    void Update()
    {
        // ������ ���ϸ� �Ѿ� �ı�
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
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
}
