using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PoliceCar : NPCBase
{

    [TabGroup("�ڵ���", "�̵�"), LabelText("���ӵ�"), SerializeField, Range(0f, 20f)]
    public float acceleration = 5f;         // ���ӵ�
    [TabGroup("�ڵ���", "�̵�"), LabelText("���� ���ӵ�"), SerializeField, Range(0f, 20f)]
    public float reverseAcceleration = 2f;  // ���� ���ӵ�
    [TabGroup("�ڵ���", "�̵�"), LabelText("���� �ִ� �ӵ�"), SerializeField, Range(0f, 20f)]
    public float maxReverseSpeed = 2f;      // ���� �� �ִ� �ӵ�

    [TabGroup("�ڵ���", "�̵�"), LabelText("ȸ�� �ӵ�"), SerializeField, Range(0f, 20f)]
    public float turnSpeed = 2f;            // ȸ�� �ӵ�
    [TabGroup("�ڵ���", "�̵�"), LabelText("�ִ� ȸ�� ����"), SerializeField, Range(0f, 60f)]
    public float maxTurnAngle = 45f;        // �ִ� ȸ�� ����

    [TabGroup("�ڵ���", "�̵�"), LabelText("�浹 �� ���� �ð�"), SerializeField, Range(0f, 4f)]
    public float reverseTime = 1f;          // ���� �ð�
    [TabGroup("�ڵ���", "�̵�"), LabelText("�浹 �� ���� �ð�"), SerializeField, Range(0f, 4f)]
    public float stopDuration = 1f;         // �浹 �� ���ߴ� �ð�

    private float currentSpeed = 0f;        // ���� �̵� �ӵ�
    private float reverseSpeed = 0f;        // ���� ���� �ӵ�
    private bool isReversing = false;       // ���� �� ����
    private bool isStoppedAfterCollision = false;  // �浹 �� ���� ����
    private bool isRush = false;
    private float reverseTimer = 0f;        // ���� �ð� Ÿ�̸�

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        agent.angularSpeed = 0f; // �ڵ����� ���� ȸ�� ó���ϹǷ� NavMesh�� ȸ���� ��Ȱ��ȭ
        agent.updateRotation = false; // ������Ʈ�� ȸ���� �������� ó����
        agent.updateUpAxis = false; // �ڵ����� ȸ���� XZ ��鿡���� �̷�������� ����
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
        if (isExplosion)
        {
            return;
        }

        if (target != null && !isReversing && !isStoppedAfterCollision && !isRush)
        {   
            // �ڵ��� �̵� ó��
            MoveCar();         
        }
        else if(target != null && isReversing && !isRush)
        {
             MoveReverse();
        }
      
    }

    protected override void MoveToTarget()
    {
        // Ÿ�� ��ġ���� ���� ����� ��ȿ�� NavMesh ��ġ�� ã�´�.
        NavMeshHit hit;
        Vector3 targetPosition = TargetGroundPos();

        // ��ȿ�� NavMesh ��ġ�� ã���� �� ��ġ�� �̵�
        if (NavMesh.SamplePosition(targetPosition, out hit, 11.0f, NavMesh.AllAreas))
        {
            targetPosition = hit.position;
        }

        // NavMesh ���� ��ȿ�� ��ġ�� �̵� �õ�
        if (!agent.SetDestination(targetPosition))
        {
            // ��� ���� ���� ��, Ÿ�� �������� ���� �Ÿ��� ���� �̵�
            Vector3 direction = (TargetPosSameYPos() - transform.position).normalized;
            Vector3 fallbackPosition = transform.position + direction * 5f; // 5�� Ÿ�� �������� �̵��� �Ÿ�

            agent.SetDestination(fallbackPosition);
        }
    }

    void MoveCar()
    {

        // MoveToTarget() ȣ��: Ÿ���� �����ϵ��� NavMeshAgent�� ��� ����
        MoveToTarget();


        // ���� ��ġ���� Ÿ�ٱ����� ���� ���
        Vector3 directionToTarget = agent.steeringTarget - transform.position;
        directionToTarget.y = 0; // Y�� �̵��� ���� (XZ ��鿡���� �̵�)

        // ���� �ӵ��� ���ӵ��� ���� ����
        if (currentSpeed < moveSpeed)
        {
            currentSpeed += acceleration * Time.deltaTime;
        }

        // �̵� ó�� (�ڽ��� �ٶ󺸴� �������θ� �̵�)
        if (currentSpeed >= 1f)
        {
            // Ÿ�� �������� �ڵ����� �ε巴�� ȸ���ϵ��� ó��
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // ȸ�� ���� ���� (�ִ� ���� ���� ����)
            targetRotation = LimitRotation(transform.rotation, targetRotation, maxTurnAngle);

            // �ӵ��� ���� ȸ�� �ӵ� ����
            float dynamicTurnSpeed = Mathf.Lerp(turnSpeed, turnSpeed * 2f, currentSpeed / moveSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, dynamicTurnSpeed * Time.deltaTime);
        }
    }

    void MoveReverse()
    {
        // ���� Ÿ�̸� ����
        if (reverseTimer < reverseTime)
        {
                
            // ���� ��ġ���� ������������ ���� ���
            Vector3 direction = TargetPosSameYPos() - transform.position;

            if (reverseSpeed >= 1f)
            {
                agent.speed = reverseSpeed;
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

        if (agent != null)
        {

            agent.nextPosition = transform.position;// ��ġ �ʱ�ȭ

            if (currentSpeed < 5)
            {
                agent.isStopped = true;
            }

            currentSpeed = 0f;
            agent.speed = currentSpeed;
        }
            // �浹 �� ��� ����
        yield return new WaitForSeconds(stopDuration);

        if (agent != null && agent.isOnNavMesh)
        {

            agent.isStopped = false;
        }

        // ���� ����
        isReversing = true;
    }

    // �浹 �̺�Ʈ ó��
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Ground") && !collision.gameObject.CompareTag("NPC"))
        {
            StartCoroutine(HandleCollision());
        }
    }
}
