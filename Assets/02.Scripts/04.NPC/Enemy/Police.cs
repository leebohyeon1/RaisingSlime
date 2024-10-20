using Pathfinding;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Police : NPCBase
{
    [BoxGroup("경찰"), LabelText("진압봉"), SerializeField]
    private GameObject baton;

    [BoxGroup("경찰"), LabelText("진압봉 흔드는 속도"), SerializeField]
    private float batonRotateSpeed;

    [BoxGroup("경찰"), LabelText("진압봉 흔드는 각도"), SerializeField]
    private float maxRotationAngle;
    
    private float currentRotationSpeed; // 현재 회전 속도


    protected override void Start()
    {
        if (target == null)
        {
            target = FindFirstObjectByType<Player>().transform;
        }

        // 초기 회전 속도 설정
        currentRotationSpeed = batonRotateSpeed;

        if (aiDestinationSetter != null)
        {
            // 타겟을 스크립트로 설정
            aiDestinationSetter.target = target;
        }

        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    protected override void enemyAction()
    {

       // base.enemyAction();


        // 현재 X축 회전 각도 계산
        float currentXRotation = baton.transform.localEulerAngles.x;

        // 회전 각도가 180도를 넘어가면 이를 보정
        if (currentXRotation > 180f)
        {
            currentXRotation -= 360f;
        }

        // 회전 각도가 최대 각도에 도달하면 회전 방향을 반대로 변경
        if (Mathf.Abs(currentXRotation) >= maxRotationAngle)
        {
            currentRotationSpeed = -currentRotationSpeed;
        }

        // 진압봉을 X축을 기준으로 회전
        baton.transform.Rotate(Vector3.right, currentRotationSpeed * Time.deltaTime);


    
    }
}
