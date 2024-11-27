using UnityEngine;
using System;
using DG.Tweening;

public class Gold : EatAbleObjectBase
{
    public Action<GameObject> OnDisableGold; // 비활성화될 때 호출할 이벤트

    private Vector3 defaultSize;
    private Tween floatTween; // 골드의 떠다니는 애니메이션
    private Tween rotateTween; // 골드의 회전 애니메이션

    private void Awake()
    {
        defaultSize = transform.localScale; // 초기 사이즈 초기화
    }

    public override void Eaten(Transform slimeTrans)
    {
        StopFloatingAndRotating();
        base.Eaten(slimeTrans);

        GameManager.Instance.IncreaseMoney();

        AudioManager.Instance.PlaySFX("EatCoin");
    }

    public override void Digested()
    {
        transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

        if (transform.localScale.x < 0.2f)
        {
            isGetEaten = !isGetEaten;
            transform.SetParent(null); // 부모를 없앰  
            gameObject.SetActive(false);
        }
    }

    private void OnEnable() // 활성화 시 스케일 변경
    {
        transform.localScale = defaultSize;
        Collider[] colliders = GetComponents<Collider>();
        isLock = false;

        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }

        StartFloatingAndRotating(); // 떠다니고 회전하는 애니메이션 시작
    }

  
    private void OnDisable()
    {
        if (OnDisableGold != null)
        {
            OnDisableGold.Invoke(gameObject);
            OnDisableGold = null; // 이벤트 초기화로 메모리 누수 방지
        }
    }

    // 떠다니고 회전하는 애니메이션 시작
    private void StartFloatingAndRotating()
    {
        // 둥둥 떠다니는 애니메이션
        floatTween = transform.DOMoveY(0.6f, 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        // 회전 애니메이션
        rotateTween = transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
    }

    // 떠다니고 회전하는 애니메이션 중단
    private void StopFloatingAndRotating()
    {
        floatTween?.Kill(); // 떠다니는 애니메이션 중단
        rotateTween?.Kill(); // 회전 애니메이션 중단
    }
}
