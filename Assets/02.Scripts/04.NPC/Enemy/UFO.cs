using INab.Dissolve;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFO : NPCBase
{
    [BoxGroup("UFO"), LabelText("�ʴ� ���ݷ�"), SerializeField, Range(0.1f, 100f)]
    private float damagePerSecond = 0.2f; 

    [BoxGroup("UFO"), LabelText("ȸ�� �ӵ�"), SerializeField]
    private float rotationSpeed = 50f; // ȸ�� �ӵ�
    
    [BoxGroup("UFO"), LabelText("���� �Ÿ�"), SerializeField]
    private float radius = 5f; // UFO�� �÷��̾�κ��� ������ �Ÿ�(�ݰ�)
    
    [BoxGroup("UFO"), LabelText("����"), SerializeField]
    private float heightFactor = 2f; // �÷��̾� ũ�⿡ ����� ���� ����

    private Player player;

    protected override void Awake()
    {

    }

    protected override void Start()
    {
        base.Start();

        player = target.GetComponent<Player>();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void enemyAction()
    {
        if (target ==  null)
        {
            return;
        }

        MoveUFO();

        Attack();
    }

    // UFO ����
    private void MoveUFO()
    {
        // �÷��̾��� ũ�⿡ ����Ͽ� ���̸� ����
        float adjustedHeight = target.localScale.y * heightFactor;

        // UFO�� �÷��̾� ������ ȸ����ŵ�ϴ�.
        transform.RotateAround(target.position, Vector3.up, rotationSpeed * Time.deltaTime);

        // �ݰ��� �����ϸ� ���̵� �÷��̾� ũ�⿡ ����Ͽ� ����
        Vector3 newPosition = transform.position - target.position;
        newPosition = newPosition.normalized * radius; // �Ÿ� ����
        newPosition.y = adjustedHeight; // ���� ����

        transform.position = target.position + newPosition; // �÷��̾� �������� ����
    }

    // ���������� �÷��̾� ����
    private void Attack()
    {
        float damage = damagePerSecond * Time.deltaTime;

        player.TakeDamage(damage);
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }
    private void OnTriggerExit(Collider other)
    {

    }
}
