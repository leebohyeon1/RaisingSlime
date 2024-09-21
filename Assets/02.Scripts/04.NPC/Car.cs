using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class Car : NPCBase
{
    private Rigidbody rb;

    [TabGroup("�ڵ���", "�̵�"), LabelText("���ӵ�"), SerializeField, Range(0f, 20f)]
    public float acceleration = 5f;         // ���ӵ�
    [TabGroup("�ڵ���", "�̵�"), LabelText("���� ���ӵ�"), SerializeField, Range(0f, 20f)]
    public float reverseAcceleration = 2f;  // ���� ���ӵ�
    [TabGroup("�ڵ���", "�̵�"), LabelText("���� �ִ� �ӵ�"), SerializeField, Range(0f, 20f)]
    public float maxReverseSpeed = 2f;      // ���� �� �ִ� �ӵ�

    [TabGroup("�ڵ���", "�̵�"), LabelText("ȸ�� �ӵ�"), SerializeField, Range(0f, 20f)]
    public float turnSpeed = 2f;            // ȸ�� �ӵ�
    [TabGroup("�ڵ���", "�̵�"), LabelText("�ִ� ȸ�� ����"), SerializeField, Range(0f, 45f)]
    public float maxTurnAngle = 45f;        // �ִ� ȸ�� ����

    [TabGroup("�ڵ���", "�̵�"), LabelText("�浹 �� ���� �ð�"), SerializeField, Range(0f, 4f)]
    public float reverseTime = 1f;          // ���� �ð�
    [TabGroup("�ڵ���", "�̵�"), LabelText("�浹 �� ���� �ð�"), SerializeField, Range(0f, 4f)]
    public float stopDuration = 1f;         // �浹 �� ���ߴ� �ð�

    private float currentSpeed = 0f;        // ���� �̵� �ӵ�
    private float reverseSpeed = 0f;        // ���� ���� �ӵ�
    private bool isReversing = false;       // ���� �� ����
    private bool isStoppedAfterCollision = false;  // �浹 �� ���� ����
    private float reverseTimer = 0f;        // ���� �ð� Ÿ�̸�


    protected override void Start()
    {
        base.Start();
        agent.angularSpeed = 0f; // �ڵ����� ���� ȸ�� ó���ϹǷ� NavMesh�� ȸ���� ��Ȱ��ȭ
        agent.updatePosition = false;   
        agent.updateRotation = false; // ������Ʈ�� ȸ���� �������� ó����
        agent.updateUpAxis = false; // �ڵ����� ȸ���� XZ ��鿡���� �̷�������� ����

        rb = GetComponent<Rigidbody>(); 
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void enemyAction()
    {
        if (eatAbleObjectBase.GetEaten())
        {
            return;
        }

        if (target != null && !isReversing && !isStoppedAfterCollision)
        {   
            // �ڵ��� �̵� ó��
            MoveCar();         
        }
        else if(target != null && isReversing)
        {
             MoveReverse();
        }
    }

    void MoveCar()
    {
        // ������ ����
        agent.SetDestination(targetGroundPos());

        // ���� ��ġ���� ������������ ���� ���
        Vector3 direction = agent.steeringTarget - transform.position;
        direction.y = 0; // Y�� �̵��� ���� (XZ ��鿡���� �̵�)

        // �̵��ؾ� �� �Ÿ��� �ſ� ª�ٸ�, �ӵ��� 0���� ����
        if (direction.magnitude < 0.5f)
        {
            currentSpeed = 0f;
            return;
        }

        // ���� �ӵ��� ���ӵ��� ���� ����
        if (currentSpeed < moveSpeed)
        {
            currentSpeed += acceleration * Time.deltaTime;
        }

        if (currentSpeed >= 1f)
        {
            // �ڵ��� ȸ�� ó�� (�ӵ��� ����Ͽ� �ε巴�� ȸ��)
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // ���� ȸ�� ������ ���� (�ִ� ���� ���� ����)
            targetRotation = LimitRotation(transform.rotation, targetRotation, maxTurnAngle);

            float dynamicTurnSpeed = Mathf.Lerp(turnSpeed, turnSpeed * 2f, currentSpeed / moveSpeed); // �ӵ��� ���� ȸ�� �ӵ� ����
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, dynamicTurnSpeed * Time.deltaTime);
        }

        rb.velocity = transform.forward * currentSpeed;
    }


    void MoveReverse()
    {
        // ���� Ÿ�̸� ����
        if (reverseTimer < reverseTime)
        {
            // ���� ��ġ���� ������������ ���� ���
            Vector3 direction = targetPosSameYPos() - transform.position;

            if (reverseSpeed >= 1f)
            {
                // �ڵ��� ȸ�� ó�� (�ӵ��� ����Ͽ� �ε巴�� ȸ��)
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // ���� ȸ�� ������ ���� (�ִ� ���� ���� ����)
                targetRotation = LimitRotation(transform.rotation, targetRotation, maxTurnAngle);

                float dynamicTurnSpeed = Mathf.Lerp(turnSpeed, turnSpeed * 2f, reverseSpeed / moveSpeed); // �ӵ��� ���� ȸ�� �ӵ� ����
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, dynamicTurnSpeed * Time.deltaTime);
            }

            // ���� ���� (NavMeshAgent�� ����Ͽ� ����)
            Vector3 reverseDirection = -transform.forward;
            Vector3 reverseTarget = transform.position + reverseDirection * 5f; // ������ ���� ���

            // ���� ������ ����
            agent.SetDestination(reverseTarget);

            reverseSpeed = Mathf.Min(reverseSpeed + reverseAcceleration * Time.deltaTime, maxReverseSpeed);

            rb.velocity = -transform.forward * reverseSpeed;

            // ���� �ð� ī��Ʈ
            reverseTimer += Time.deltaTime;
        }
        else
        {
            // ���� ���� �� �ʱ�ȭ
            isReversing = false;
            isStoppedAfterCollision = false;
            reverseTimer = 0f;
            reverseSpeed = 0f;
            agent.speed = moveSpeed; // �ӵ��� ������� ����
        }
    }

    // �ִ� ȸ�� ���� ���� �Լ�
    Quaternion LimitRotation(Quaternion currentRotation, Quaternion targetRotation, float maxAngle)
    {
        // ���� ȸ���� ��ǥ ȸ�� ������ ������ ���
        float angle = Quaternion.Angle(currentRotation, targetRotation);

        // �ִ� ������ �ʰ��� ��� ������ ����
        if (angle > maxAngle)
        {
            return Quaternion.RotateTowards(currentRotation, targetRotation, maxAngle);
        }

        // ������ �ִ� ���� �̳���� �״�� ��ȯ
        return targetRotation;
    }


    // �浹 �� ����, ����, �簳�ϴ� �ڷ�ƾ
    IEnumerator HandleCollision()
    {
        isStoppedAfterCollision = true;
        currentSpeed = 0f;

        // �浹 �� ��� ����
        yield return new WaitForSeconds(stopDuration);

        // ���� ����
        isReversing = true;
    }

    // �浹 �̺�Ʈ ó��
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
        {
            StartCoroutine(HandleCollision());
        }
    }
}
