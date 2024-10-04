using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PoliceCar : NPCBase
{

    [TabGroup("자동차", "이동"), LabelText("가속도"), SerializeField, Range(0f, 20f)]
    public float acceleration = 5f;         // 가속도
    [TabGroup("자동차", "이동"), LabelText("후진 가속도"), SerializeField, Range(0f, 20f)]
    public float reverseAcceleration = 2f;  // 후진 가속도
    [TabGroup("자동차", "이동"), LabelText("후진 최대 속도"), SerializeField, Range(0f, 20f)]
    public float maxReverseSpeed = 2f;      // 후진 시 최대 속도

    [TabGroup("자동차", "이동"), LabelText("회전 속도"), SerializeField, Range(0f, 20f)]
    public float turnSpeed = 2f;            // 회전 속도
    [TabGroup("자동차", "이동"), LabelText("최대 회전 각도"), SerializeField, Range(0f, 60f)]
    public float maxTurnAngle = 45f;        // 최대 회전 각도

    [TabGroup("자동차", "이동"), LabelText("충돌 후 후진 시간"), SerializeField, Range(0f, 4f)]
    public float reverseTime = 1f;          // 후진 시간
    [TabGroup("자동차", "이동"), LabelText("충돌 후 정지 시간"), SerializeField, Range(0f, 4f)]
    public float stopDuration = 1f;         // 충돌 후 멈추는 시간

    private float currentSpeed = 0f;        // 현재 이동 속도
    private float reverseSpeed = 0f;        // 현재 후진 속도
    private bool isReversing = false;       // 후진 중 여부
    private bool isStoppedAfterCollision = false;  // 충돌 후 멈춤 여부
    private bool isRush = false;
    private float reverseTimer = 0f;        // 후진 시간 타이머

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        agent.angularSpeed = 0f; // 자동차는 직접 회전 처리하므로 NavMesh의 회전을 비활성화
        agent.updateRotation = false; // 에이전트의 회전은 수동으로 처리함
        agent.updateUpAxis = false; // 자동차의 회전은 XZ 평면에서만 이루어지도록 설정
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
            // 자동차 이동 처리
            MoveCar();         
        }
        else if(target != null && isReversing && !isRush)
        {
             MoveReverse();
        }
      
    }

    protected override void MoveToTarget()
    {
        // 타겟 위치에서 가장 가까운 유효한 NavMesh 위치를 찾는다.
        NavMeshHit hit;
        Vector3 targetPosition = TargetGroundPos();

        // 유효한 NavMesh 위치를 찾으면 그 위치로 이동
        if (NavMesh.SamplePosition(targetPosition, out hit, 11.0f, NavMesh.AllAreas))
        {
            targetPosition = hit.position;
        }

        // NavMesh 상의 유효한 위치로 이동 시도
        if (!agent.SetDestination(targetPosition))
        {
            // 경로 설정 실패 시, 타겟 방향으로 일정 거리를 더해 이동
            Vector3 direction = (TargetPosSameYPos() - transform.position).normalized;
            Vector3 fallbackPosition = transform.position + direction * 5f; // 5는 타겟 방향으로 이동할 거리

            agent.SetDestination(fallbackPosition);
        }
    }

    void MoveCar()
    {

        // MoveToTarget() 호출: 타겟을 추적하도록 NavMeshAgent에 경로 설정
        MoveToTarget();


        // 현재 위치에서 타겟까지의 방향 계산
        Vector3 directionToTarget = agent.steeringTarget - transform.position;
        directionToTarget.y = 0; // Y축 이동은 무시 (XZ 평면에서만 이동)

        // 현재 속도를 가속도에 맞춰 증가
        if (currentSpeed < moveSpeed)
        {
            currentSpeed += acceleration * Time.deltaTime;
        }

        // 이동 처리 (자신이 바라보는 방향으로만 이동)
        if (currentSpeed >= 1f)
        {
            // 타겟 방향으로 자동차가 부드럽게 회전하도록 처리
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // 회전 각도 제한 (최대 각도 제한 적용)
            targetRotation = LimitRotation(transform.rotation, targetRotation, maxTurnAngle);

            // 속도에 따른 회전 속도 조정
            float dynamicTurnSpeed = Mathf.Lerp(turnSpeed, turnSpeed * 2f, currentSpeed / moveSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, dynamicTurnSpeed * Time.deltaTime);
        }
    }

    void MoveReverse()
    {
        // 후진 타이머 관리
        if (reverseTimer < reverseTime)
        {
                
            // 현재 위치에서 목적지까지의 방향 계산
            Vector3 direction = TargetPosSameYPos() - transform.position;

            if (reverseSpeed >= 1f)
            {
                agent.speed = reverseSpeed;
                // 자동차 회전 처리 (속도에 비례하여 부드럽게 회전)
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // 현재 회전 각도를 제한 (최대 각도 제한 적용)
                targetRotation = LimitRotation(transform.rotation, targetRotation, maxTurnAngle);

                float dynamicTurnSpeed = Mathf.Lerp(turnSpeed, turnSpeed * 2f, reverseSpeed / moveSpeed); // 속도에 따른 회전 속도 조정
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, dynamicTurnSpeed * Time.deltaTime);
            }

            // 후진 방향 (NavMeshAgent를 사용하여 후진)
            Vector3 reverseDirection = -transform.forward;
            Vector3 reverseTarget = transform.position + reverseDirection * 5f; // 후진할 지점 계산

            // 후진 목적지 설정
            agent.SetDestination(reverseTarget);

            reverseSpeed = Mathf.Min(reverseSpeed + reverseAcceleration * Time.deltaTime, maxReverseSpeed);

            // 후진 시간 카운트
            reverseTimer += Time.deltaTime;
        }
        else
        {
            // 후진 종료 후 초기화
            isReversing = false;
            isStoppedAfterCollision = false;
            reverseTimer = 0f;
            reverseSpeed = 0f;
            agent.speed = moveSpeed; // 속도를 원래대로 복원
        }
    }

    // 최대 회전 각도 제한 함수
    Quaternion LimitRotation(Quaternion currentRotation, Quaternion targetRotation, float maxAngle)
    {
        // 현재 회전과 목표 회전 사이의 각도를 계산
        float angle = Quaternion.Angle(currentRotation, targetRotation);

        // 최대 각도를 초과할 경우 각도를 제한
        if (angle > maxAngle)
        {
            return Quaternion.RotateTowards(currentRotation, targetRotation, maxAngle);
        }

        // 각도가 최대 각도 이내라면 그대로 반환
        return targetRotation;
    }


    // 충돌 후 멈춤, 후진, 재개하는 코루틴
    IEnumerator HandleCollision()
    {
      

        isStoppedAfterCollision = true;

        if (agent != null)
        {

            agent.nextPosition = transform.position;// 위치 초기화

            if (currentSpeed < 5)
            {
                agent.isStopped = true;
            }

            currentSpeed = 0f;
            agent.speed = currentSpeed;
        }
            // 충돌 후 잠시 멈춤
        yield return new WaitForSeconds(stopDuration);

        if (agent != null && agent.isOnNavMesh)
        {

            agent.isStopped = false;
        }

        // 후진 로직
        isReversing = true;
    }

    // 충돌 이벤트 처리
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Ground") && !collision.gameObject.CompareTag("NPC"))
        {
            StartCoroutine(HandleCollision());
        }
    }
}
