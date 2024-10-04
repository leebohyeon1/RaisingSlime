using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Police : NPCBase
{
    [BoxGroup("����"), LabelText("���к�"), SerializeField]
    private GameObject baton;

    [BoxGroup("����"), LabelText("���к� ���� �ӵ�"), SerializeField]
    private float batonRotateSpeed;

    [BoxGroup("����"), LabelText("���к� ���� ����"), SerializeField]
    private float maxRotationAngle;
    
    private float currentRotationSpeed; // ���� ȸ�� �ӵ�

    protected override void Start()
    {
        base.Start();

        // �ʱ� ȸ�� �ӵ� ����
        currentRotationSpeed = batonRotateSpeed;
    }


    protected override void Update()
    {
        base.Update();
    }

    protected override void enemyAction()
    {

        base.enemyAction();


        // ���� X�� ȸ�� ���� ���
        float currentXRotation = baton.transform.localEulerAngles.x;

        // ȸ�� ������ 180���� �Ѿ�� �̸� ����
        if (currentXRotation > 180f)
        {
            currentXRotation -= 360f;
        }

        // ȸ�� ������ �ִ� ������ �����ϸ� ȸ�� ������ �ݴ�� ����
        if (Mathf.Abs(currentXRotation) >= maxRotationAngle)
        {
            currentRotationSpeed = -currentRotationSpeed;
        }

        // ���к��� X���� �������� ȸ��
        baton.transform.Rotate(Vector3.right, currentRotationSpeed * Time.deltaTime);


    
    }
}