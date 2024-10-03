using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class TankBullet : Bullet
{
    [TabGroup("��ź", "�̵�"), LabelText("�߷�"), SerializeField, Range(0.1f, 20f)]
    private float gravity = 9.81f; // �ϰ��� �� ����� ���ӵ�
    // [TabGroup("��ź", "�̵�"), LabelText("������ �� �÷��̾���� �Ÿ�"), SerializeField, Range(0.1f, 20f)]
    // public float fallDistance = 3f; // �÷��̾���� �Ÿ��� �� ������ �۾����� ��ź�� �������� ����
    
    [TabGroup("��ź", "����"), LabelText("���� ������"), SerializeField, Range(0.1f, 100f)]
    public float explosionForce = 5f; // ������ �� �������� ��
    [TabGroup("��ź", "����"), LabelText("���� ����"), SerializeField, Range(0.1f, 100f)]
    public float explosionRadius = 5f; // ���� ����
    [TabGroup("��ź", "����"), LabelText("���� �� ���� ƨ��� ��"), SerializeField, Range(0.1f, 20f)]
    public float upwardsModifier = 1f; // ���� �� ���� ƨ�ܳ����� ���� ����
    [TabGroup("��ź", "����"), LabelText("���߿� ����޴� ���̾�"), SerializeField]
    public LayerMask explosionLayerMask; // ������ ������ ���� ���̾� ����ũ (�÷��̾�, NPC ��)

    private Vector3 targetPos = Vector3.zero; // Ÿ���� �Ǵ� �÷��̾� �Ǵ� ������
    private Vector3 fallPos;
    private bool isFalling = false;

    // Start is called before the first frame update
    protected override void Awake()
    {
        rb = GetComponent<Rigidbody>();

        lifeTimer = lifetime; // ���� Ÿ�̸� �ʱ�ȭ
    }

    protected override void Update()
    {
        base.Update();

        if (!isFalling)
        {
            // float distanceToPlayer = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPos.x, 0, targetPos.z));

            float distanceSqrd = (new Vector3(transform.position.x, fallPos.y, transform.position.z) - fallPos).sqrMagnitude;
            // �÷��̾���� �Ÿ��� ������ fallDistance���� �۾����� �ϰ� ����
            if (distanceSqrd <= 0.3f)
            {
                isFalling = true;
                StartFalling();
            }
        }
    }

    public void InitialTarget(Vector3 target, float force, float radius, float upModifier, LayerMask layer)
    {
        targetPos = target;
        explosionForce = force;
        explosionRadius = radius;
        upwardsModifier = upModifier;
        explosionLayerMask = layer;

        fallPos = Vector3.Lerp(transform.position, new Vector3(targetPos.x, 0, targetPos.z), 0.5f);
   
    }

    // ��ź�� ���������� ����� �Լ�
    void StartFalling()
    {
        // ��ź�� Y�� �ӵ��� �߷� ���ӵ��� �߰��Ͽ� �ϰ��ϰ� ����
        rb.useGravity = true;

        // ���� �ӵ� �̻��� �� �߰� �߷� ����
        if (rb.velocity.magnitude > gravity)
        {
            // �ӵ��� ������ �߷��� �� ���� ����
            rb.velocity += Vector3.down * (rb.velocity.magnitude - gravity) * 1.5f;
        }
   
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
                //npc.TakeDamage(damage); // NPC�� ������ ����
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

        Gizmos.DrawLine(fallPos, Vector3.up);
    }

    protected override void OnTriggerEnter(Collider collision)
    {
        Explode();
    }


}
