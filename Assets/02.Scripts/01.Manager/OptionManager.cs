using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
    public static OptionManager Instance { get; private set; } // �̱��� ����

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

            // RectTransform���� ó�� ��ġ�� ������
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

            // ȭ�� �Ʒ� ��ġ ��� (���� �ڵ�� �̵�)
            Vector2 belowScreenPos = new Vector2(optionRect.anchoredPosition.x, -(Screen.height / 2 + optionRect.rect.height));
            Vector2 btnBelowScreenPos = new Vector2(btnRect.anchoredPosition.x, belowScreenPos.y);

            // �ִϸ��̼� ������
            Sequence exitOptionSequence = DOTween.Sequence();

            exitOptionSequence.Append(btnRect.DOAnchorPos(btnBelowScreenPos, 0.6f).SetEase(Ease.InBack, 0.5f));
            exitOptionSequence.Insert(0.1f, optionRect.DOAnchorPos(belowScreenPos, 0.6f).SetEase(Ease.InBack, 1.7f));

            // �ִϸ��̼� �Ϸ� ��, ���� �ڸ��� �����ϰ� ��Ȱ��ȭ
            exitOptionSequence.OnComplete(() =>
            {
                // ��ġ�� ��� ���� �ڸ��� ����
                optionRect.anchoredPosition = optionOriginalPos;
                btnRect.anchoredPosition = btnOriginalPos;

                

                option.SetActive(false); // �ɼ� UI ����
                
                if (GameManager.Instance)
                {
                    GameManager.Instance.SetOption();
                }
                
            });

            // Ÿ�� �����Ͽ� ������� �ִϸ��̼��� �۵��ϵ��� ����
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

            Vector2 offScreenPos = new Vector2(optionRect.anchoredPosition.x, Screen.height / 2 + optionRect.rect.height); // ȭ�� ���� ���� ��ġ
            Vector2 btnOffScreenPos = new Vector2(btnRect.anchoredPosition.x, Screen.height / 2 + btnRect.rect.height); // ȭ�� ���� ���� ��ġ

            optionRect.anchoredPosition = offScreenPos;
            btnRect.anchoredPosition = btnOffScreenPos;

            Sequence enterOptionSequence = DOTween.Sequence();

            enterOptionSequence.Append(btnRect.DOAnchorPos(btnOriginalPos, 0.8f).SetEase(Ease.OutElastic, 1.2f, 0.6f));
            enterOptionSequence.Insert(0.1f, optionRect.DOAnchorPos(optionOriginalPos, 0.6f).SetEase(Ease.OutElastic, 1.2f, 0.6f));


            enterOptionSequence.SetUpdate(true);
        }
    }
}
