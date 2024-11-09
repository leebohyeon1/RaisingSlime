using Pathfinding;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PoliceCar : NPCBase
{
    [TabGroup("자동차", "이동"), LabelText("가속도"), SerializeField, Range(0f, 20f)]
    public float acceleration = 5f;
    [TabGroup("자동차", "이동"), LabelText("후진 가속도"), SerializeField, Range(0f, 20f)]
    public float reverseAcceleration = 2f;
    [TabGroup("자동차", "이동"), LabelText("후진 최대 속도"), SerializeField, Range(0f, 20f)]
    public float maxReverseSpeed = 2f;
    [TabGroup("자동차", "이동"), LabelText("회전 속도"), SerializeField, Range(0f, 20f)]
    public float turnSpeed = 2f;
    [TabGroup("자동차", "이동"), LabelText("최대 회전 각도"), SerializeField, Range(0f, 60f)]
    public float maxTurnAngle = 45f;
    [TabGroup("자동차", "이동"), LabelText("충돌 후 후진 시간"), SerializeField, Range(0f, 4f)]
    public float reverseTime = 1f;
    [TabGroup("자동차", "이동"), LabelText("충돌 후 정지 시간"), SerializeField, Range(0f, 4f)]
    public float stopDuration = 1f;

    [TabGroup("자동차", "이동"), LabelText("후진 위치")]
    public Transform backPos;

    private bool isReversing = false;
    private float reverseTimer = 0f;
    private Coroutine collisionCoroutine; // 현재 실행 중인 코루틴을 저장

    protected override void Awake()
    {
        base.Awake();
        aiPath.maxAcceleration = acceleration;
        aiPath.updateRotation = false;
        aiPath.enableRotation = false;
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

            if (isReversing)
                MoveReverse();
            else
                MoveCar();
        }
       
    }

    private void MoveCar()
    {
        CheckDistanceToTarget();
        MoveToTarget();

        Vector3 directionToTarget = aiPath.steeringTarget - transform.position;
        directionToTarget.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        targetRotation = LimitRotation(transform.rotation, targetRotation, maxTurnAngle);

        float dynamicTurnSpeed = Mathf.Lerp(turnSpeed, turnSpeed * 2f, aiPath.velocity.magnitude / moveSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, dynamicTurnSpeed * Time.deltaTime);
    }

    private void MoveReverse()
    {
        if (reverseTimer < reverseTime)
        { 

            Vector3 direction = TargetPosSameYPos() - transform.position;
            aiPath.isStopped = false;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation = LimitRotation(transform.rotation, targetRotation, maxTurnAngle);

            float dynamicTurnSpeed = Mathf.Lerp(turnSpeed, turnSpeed * 2f, aiPath.velocity.magnitude / moveSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, dynamicTurnSpeed * Time.deltaTime);

            aiPath.destination = backPos.position;
            reverseTimer += Time.deltaTime;
        }
        else
        {
            ResetAfterReverse();
        }
    }

    private void ResetAfterReverse()
    {
        isReversing = false;
        reverseTimer = 0f;

        aiPath.maxSpeed = moveSpeed;
        aiPath.maxAcceleration = acceleration;

        aiPath.isStopped = false; // 이동 재개
        aiPath.destination = target.position; // 타겟을 원래 목표로 다시 설정
    }

    private Quaternion LimitRotation(Quaternion currentRotation, Quaternion targetRotation, float maxAngle)
    {
        float angle = Quaternion.Angle(currentRotation, targetRotation);
        if (angle > maxAngle)
            return Quaternion.RotateTowards(currentRotation, targetRotation, maxAngle);
        return targetRotation;
    }

    private IEnumerator HandleCollision()
    {
        aiPath.isStopped = true;
        aiPath.maxSpeed = 0f;

        Rigidbody rb = GetComponent<Rigidbody>();

        yield return new WaitForSeconds(stopDuration);

        aiPath.maxSpeed = maxReverseSpeed;
        aiPath.maxAcceleration = reverseAcceleration;

        isReversing = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (/*!collision.gameObject.CompareTag("Ground") && !collision.gameObject.CompareTag("NPC")*/ collision.gameObject.CompareTag("Player"))
        {
            // 이전에 실행 중인 코루틴이 있으면 정지
            if (collisionCoroutine != null)
                StopCoroutine(collisionCoroutine);

            // 새로운 코루틴 시작
            collisionCoroutine = StartCoroutine(HandleCollision());
        }
    }
}
