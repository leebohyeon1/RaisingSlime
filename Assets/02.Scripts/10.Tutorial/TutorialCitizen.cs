using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCitizen : EatAbleObjectBase
{
    [SerializeField]
    private Transform arrowObj;

    private bool isJump = false;
    private bool isMove = false;

    private Vector3[] defaultTrans = new Vector3[2];

    protected override void Start()
    {
        defaultTrans[0] = transform.position;
        defaultTrans[1] = arrowObj.position;

        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    private void Update()
    {
        if (GetEaten())
        {
            return;
        }

        if(!isJump)
        {
            isJump = true;
            LoopJump();
        }

        if (!isMove)
        {
            isMove = true;
            LoopArrow();
        }
    }

    void LoopJump()
    {
        transform.DOMoveY(transform.position.y + 0.6f, 0.25f).SetEase(Ease.OutQuad)
            .OnComplete(()=> { transform.DOMoveY(defaultTrans[0].y, 0.25f).SetEase(Ease.InQuad).
                OnComplete(()=> { isJump = false; });  });
    }

    void LoopArrow()
    {
        arrowObj.DOMoveY(arrowObj.position.y + 0.2f, 0.4f).SetEase(Ease.InOutSine)
            .OnComplete(() => { arrowObj.DOMoveY(defaultTrans[1].y , 0.4f).SetEase(Ease.InOutSine)
                .OnComplete(()=> { isMove = false; }); });
    }

    public override void Eaten(Transform slimeTrans)
    {
        base.Eaten(slimeTrans);

        Destroy(arrowObj.gameObject);
    }


}
