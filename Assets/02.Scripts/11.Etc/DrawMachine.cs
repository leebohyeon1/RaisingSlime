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
    private bool isRotatingArm = true; // 회전 방향 추적

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

        float shakeAngle = 15f; // x축 각도
        float direction = isRotatingArm ? 1f : -1f; // 방향 결정
        float shakeDuration = 2f; // 반대 방향으로 전환할 시간

        shakeTimer += Time.deltaTime;

        // 회전
        rightArm.Rotate(Vector3.right, direction * shakeAngle * Time.deltaTime);

        // 일정 시간이 지나면 방향을 반대로 변경
        if (shakeTimer >= shakeDuration)
        {
            shakeTimer = 0f; // 타이머 초기화
            isRotatingArm = !isRotatingArm; // 방향 전환
        }
    }

}
