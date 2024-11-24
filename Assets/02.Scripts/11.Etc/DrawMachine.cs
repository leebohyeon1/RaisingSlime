using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DrawMachine : MonoBehaviour
{
    [SerializeField] private Transform capsulePivot;

    [SerializeField] private float shakeForce;

    [SerializeField] private Transform rightArm;
    [SerializeField] private DrawManager drawManager;

    private bool isDraw;
    private float timer = 0.3f;
    private float shakeTimer;
    private bool isRotatingArm = true; // ȸ�� ���� ����

    private LayerMask layerMask = 1 << 4;


    private void Update()
    {
        if(isDraw)
        {
            timer += Time.deltaTime;
            if (timer > 0.45f)
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
        timer = 0.3f;
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
