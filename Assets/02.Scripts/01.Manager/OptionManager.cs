using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
    public static OptionManager Instance { get; private set; } // 싱글턴 선언

    public GameObject option;
    public GameObject optionUI;
    public GameObject exitBtn;

    private Vector2 optionOriginalPos;
    private Vector2 btnOriginalPos;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // RectTransform에서 처음 위치를 가져옴
            optionOriginalPos = optionUI.GetComponent<RectTransform>().anchoredPosition;
            btnOriginalPos = exitBtn.GetComponent<RectTransform>().anchoredPosition;
            
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }


    }

    public void ExitOption()
    {
        exitBtn.GetComponent<Button>().interactable = false;

        if (option.activeSelf)
        {
            RectTransform optionRect = optionUI.GetComponent<RectTransform>();
            RectTransform btnRect = exitBtn.GetComponent<RectTransform>();

            // 화면 아래 위치 계산 (공통 코드로 이동)
            Vector2 belowScreenPos = new Vector2(optionRect.anchoredPosition.x, -(Screen.height / 2 + optionRect.rect.height));
            Vector2 btnBelowScreenPos = new Vector2(btnRect.anchoredPosition.x, belowScreenPos.y);

            // 애니메이션 시퀀스
            Sequence exitOptionSequence = DOTween.Sequence();

            exitOptionSequence.Append(btnRect.DOAnchorPos(btnBelowScreenPos, 0.6f).SetEase(Ease.InBack, 0.5f));
            exitOptionSequence.Insert(0.1f, optionRect.DOAnchorPos(belowScreenPos, 0.6f).SetEase(Ease.InBack, 1.7f));

            // 애니메이션 완료 후, 원래 자리로 복귀하고 비활성화
            exitOptionSequence.OnComplete(() =>
            {
                // 위치를 즉시 원래 자리로 설정
                optionRect.anchoredPosition = optionOriginalPos;
                btnRect.anchoredPosition = btnOriginalPos;

                

                option.SetActive(false); // 옵션 UI 끄기
                
                if (GameManager.Instance)
                {
                    GameManager.Instance.SetOption();
                }
                
            });

            // 타임 스케일에 상관없이 애니메이션이 작동하도록 설정
            exitOptionSequence.SetUpdate(true);
        }
    }

    public void EnterOption()
    {
        exitBtn.GetComponent<Button>().interactable = true;
        if (!option.activeSelf)
        {
            RectTransform optionRect = optionUI.GetComponent<RectTransform>();
            RectTransform btnRect = exitBtn.GetComponent<RectTransform>();

            optionRect.anchoredPosition = optionOriginalPos;
            btnRect.anchoredPosition = btnOriginalPos;

            option.SetActive(true);

            Vector2 offScreenPos = new Vector2(optionRect.anchoredPosition.x, Screen.height / 2 + optionRect.rect.height); // 화면 위의 임의 위치
            Vector2 btnOffScreenPos = new Vector2(btnRect.anchoredPosition.x, Screen.height / 2 + btnRect.rect.height); // 화면 위의 임의 위치

            optionRect.anchoredPosition = offScreenPos;
            btnRect.anchoredPosition = btnOffScreenPos;

            Sequence enterOptionSequence = DOTween.Sequence();

            enterOptionSequence.Append(btnRect.DOAnchorPos(btnOriginalPos, 0.8f).SetEase(Ease.OutElastic, 1.2f, 0.6f));
            enterOptionSequence.Insert(0.1f, optionRect.DOAnchorPos(optionOriginalPos, 0.6f).SetEase(Ease.OutElastic, 1.2f, 0.6f));


            enterOptionSequence.SetUpdate(true);
        }
    }
}
