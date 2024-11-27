using UnityEngine;
using System;
using DG.Tweening;

public class Gold : EatAbleObjectBase
{
    public Action<GameObject> OnDisableGold; // ��Ȱ��ȭ�� �� ȣ���� �̺�Ʈ

    private Vector3 defaultSize;
    private Tween floatTween; // ����� ���ٴϴ� �ִϸ��̼�
    private Tween rotateTween; // ����� ȸ�� �ִϸ��̼�

    private void Awake()
    {
        defaultSize = transform.localScale; // �ʱ� ������ �ʱ�ȭ
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
            transform.SetParent(null); // �θ� ����  
            gameObject.SetActive(false);
        }
    }

    private void OnEnable() // Ȱ��ȭ �� ������ ����
    {
        transform.localScale = defaultSize;
        Collider[] colliders = GetComponents<Collider>();
        isLock = false;

        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }

        StartFloatingAndRotating(); // ���ٴϰ� ȸ���ϴ� �ִϸ��̼� ����
    }

  
    private void OnDisable()
    {
        if (OnDisableGold != null)
        {
            OnDisableGold.Invoke(gameObject);
            OnDisableGold = null; // �̺�Ʈ �ʱ�ȭ�� �޸� ���� ����
        }
    }

    // ���ٴϰ� ȸ���ϴ� �ִϸ��̼� ����
    private void StartFloatingAndRotating()
    {
        // �յ� ���ٴϴ� �ִϸ��̼�
        floatTween = transform.DOMoveY(0.6f, 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        // ȸ�� �ִϸ��̼�
        rotateTween = transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
    }

    // ���ٴϰ� ȸ���ϴ� �ִϸ��̼� �ߴ�
    private void StopFloatingAndRotating()
    {
        floatTween?.Kill(); // ���ٴϴ� �ִϸ��̼� �ߴ�
        rotateTween?.Kill(); // ȸ�� �ִϸ��̼� �ߴ�
    }
}
