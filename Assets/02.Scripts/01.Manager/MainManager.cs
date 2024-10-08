using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [LabelText("���� UI"), SerializeField]
    private GameObject mainUI;
    [LabelText("�ɼ� UI"), SerializeField]
    private GameObject optionUI;

    [LabelText("���� ��ư"), SerializeField]
    private Button[] mainBtn;
    [LabelText("���� ���� �� ��ư"), SerializeField]
    private Button startPushBtn;

    void Start()
    {
        if (optionUI == null)
        {
            optionUI = OptionManager.Instance.optionUI;
        }

        mainUI.SetActive(true);

        startPushBtn.gameObject.SetActive(false);
    }

    void Update()
    {
        
    }

    public void StartButton()  // ���� ���� ��ư
    {
        foreach(Button btn in mainBtn)
        {
            btn.interactable = false; //��ư �� ����ȭ
        }

        startPushBtn.gameObject.SetActive(true);

        /*
        RectTransform btnRect1 = mainBtn[0].GetComponent<RectTransform>();
        RectTransform btnRect2 = mainBtn[1].GetComponent<RectTransform>();
        RectTransform btnRect3 = mainBtn[2].GetComponent<RectTransform>();

        // UI�� ȭ�� ���� �ö� �� �Ʒ��� �������� �ִϸ��̼�
        Vector2 belowScreenPos1 = new Vector2(btnRect1.anchoredPosition.x, -(Screen.height / 2 + btnRect1.rect.height)); // ȭ�� �Ʒ��� ���� ��ġ
        Vector2 belowScreenPos2 = new Vector2(btnRect2.anchoredPosition.x, -(Screen.height / 2 + btnRect2.rect.height)); // ȭ�� �Ʒ��� ���� ��ġ
        Vector2 belowScreenPos3 = new Vector2(btnRect3.anchoredPosition.x, -(Screen.height / 2 + btnRect3.rect.height)); // ȭ�� �Ʒ��� ���� ��ġ

        // �ִϸ��̼� ������
        Sequence startSequence = DOTween.Sequence();

        // ȭ�� ���� ��¦ �ö󰬴ٰ� �Ʒ��� �������� �ִϸ��̼�
        startSequence.Append(btnRect1.DOAnchorPos(belowScreenPos1, 0.6f).SetEase(Ease.InBack, 2)); // ȭ�� �Ʒ��� ������
        startSequence.Insert(0.1f,btnRect2.DOAnchorPos(belowScreenPos2, 0.6f).SetEase(Ease.InBack, 2)); // ȭ�� �Ʒ��� ������
        startSequence.Join(btnRect3.DOAnchorPos(belowScreenPos3, 0.6f).SetEase(Ease.InBack, 2)); // ȭ�� �Ʒ��� ������

        // �ִϸ��̼� �Ϸ� �� Pause UI ��Ȱ��ȭ
        startSequence.OnComplete(() =>
        {*/

        
        //SceneManager.LoadScene(2);

        /*});*/


    }

    public void ExitButton()   // ���� ���� ��ư
    {
        Application.Quit();
    }

    public void OptionButton() // ���� ��ư
    {
        OptionManager.Instance.EnterOption();    
    }
}
