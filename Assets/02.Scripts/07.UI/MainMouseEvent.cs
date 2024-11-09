using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMouseEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 defaultScale;

    private void Start()
    {
        defaultScale = transform.localScale;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {      
        transform.DOScale(defaultScale * 1.05f,0.2f).SetEase(Ease.InSine);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(defaultScale, 0.2f).SetEase(Ease.InSine);
    }
}
