using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCitizen : MonoBehaviour
{
    [SerializeField]
    private float jumpDuration = 0.5f;

    private bool isJump = false;

    private Vector3[] defaultTrans = new Vector3[2];

    protected void Start()
    {
        defaultTrans[0] = transform.position;
    }

    private void Update()
    {
        if (!isJump)
        {
            isJump = true;
            LoopJump();
        }
}

void LoopJump()
    {
        transform.DOMoveY(transform.position.y + 0.6f, jumpDuration).SetEase(Ease.OutQuad)
            .OnComplete(()=> { transform.DOMoveY(defaultTrans[0].y, jumpDuration).SetEase(Ease.InQuad).
                OnComplete(()=> { isJump = false; });  });
    }
}
