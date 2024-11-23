using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMachine : MonoBehaviour
{
    [SerializeField] private Transform capsulePivot;

    [SerializeField] private float shakeForce;

    [SerializeField] private Transform rightArm;

    private bool isDraw;
    private float timer;
    private float shakeTimer;
    private bool isRotatingArm = true; // ȸ�� ���� ����
    private bool isRotatingTransform = true; // ȸ�� ���� ����

    private LayerMask layerMask = 1 << 4;

    private void Start()
    {
        
    }

    
    private void Update()
    {
        if(isDraw)
        {
            timer += Time.deltaTime;
            if (timer > 0.3f)
            {
                timer = 0f;
                ShakeCapsule();

            }
            return;
        }

        ShakeHand();
    }

    public void SetDraw()
    {
        isDraw = !isDraw;
    }

    private void ShakeCapsule()
    {
        Collider[] capsules = Physics.OverlapSphere(capsulePivot.position, 1f, layerMask);
        foreach (Collider c in capsules)
        {
            c.GetComponent<Rigidbody>().AddExplosionForce(Random.Range(shakeForce, shakeForce * 1.2f), capsulePivot.position,20f,30f);
        } 
    }

    private void ShakeHand()
    {
        if (rightArm == null) return;

        float shakeAngle = 15f; // x�� ����
        float direction = isRotatingArm ? 1f : -1f; // ���� ����
        float shakeDuration = 2f; // �ݴ� �������� ��ȯ�� �ð�

        shakeTimer += Time.deltaTime;

        // ȸ��
        rightArm.Rotate(Vector3.right, direction * shakeAngle * Time.deltaTime);

        // ���� �ð��� ������ ������ �ݴ�� ����
        if (shakeTimer >= shakeDuration)
        {
            shakeTimer = 0f; // Ÿ�̸� �ʱ�ȭ
            isRotatingArm = !isRotatingArm; // ���� ��ȯ
        }
    }

}
