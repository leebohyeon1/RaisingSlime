using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [TabGroup("UI"), LabelText("���� UI"), SerializeField]
    private GameObject mainUI;
    //[LabelText("�ɼ� UI"), SerializeField]
    //private GameObject optionUI;

    [TabGroup("UI", "��ư"), LabelText("���� ��ư"), SerializeField]
    private Button[] mainBtn;

    [TabGroup("UI", "����"), LabelText("�ְ� ���� text"), SerializeField]
    private TMP_Text bestScoreText;

    [TabGroup("UI", "��"), LabelText("�ְ� ���� text"), SerializeField]
    private TMP_Text moneyText;


    [BoxGroup("���� ������"), LabelText("���� ��"), SerializeField]
    private uint money;
    [BoxGroup("���� ������"), LabelText("�ְ� ����"), SerializeField]
    private uint bestScore;

    private bool isOption;
    
    void Start()
    {
        //if (optionUI == null)
        //{
        //    optionUI = OptionManager.Instance.optionUI;
        //}

        LoadData();

        mainUI.SetActive(true);

        // �� ��ư�� Ŭ�� �̺�Ʈ�� �ִϸ��̼� �߰�
        //foreach (Button btn in mainBtn)
        //{
        //    btn.onClick.AddListener(() => AnimateButton(btn));
        //}
    }

    #region button
    public void StartButton()  // ���� ���� ��ư
    {
        DisableButtons();

        //RectTransform btnRect1 = mainBtn[0].GetComponent<RectTransform>();
        //RectTransform btnRect2 = mainBtn[1].GetComponent<RectTransform>();
        //RectTransform btnRect3 = mainBtn[2].GetComponent<RectTransform>();

        //// UI�� ȭ�� ���� �ö� �� �Ʒ��� �������� �ִϸ��̼�
        //Vector2 belowScreenPos1 = new Vector2(btnRect1.anchoredPosition.x, -(Screen.height / 2 + btnRect1.rect.height)); // ȭ�� �Ʒ��� ���� ��ġ
        //Vector2 belowScreenPos2 = new Vector2(btnRect2.anchoredPosition.x, -(Screen.height / 2 + btnRect2.rect.height)); // ȭ�� �Ʒ��� ���� ��ġ
        //Vector2 belowScreenPos3 = new Vector2(btnRect3.anchoredPosition.x, -(Screen.height / 2 + btnRect3.rect.height)); // ȭ�� �Ʒ��� ���� ��ġ

        //// �ִϸ��̼� ������
        //Sequence startSequence = DOTween.Sequence();

        //// ȭ�� ���� ��¦ �ö󰬴ٰ� �Ʒ��� �������� �ִϸ��̼�
        //startSequence.Append(btnRect1.DOAnchorPos(belowScreenPos1, 0.6f).SetEase(Ease.InBack, 2)); // ȭ�� �Ʒ��� ������
        //startSequence.Insert(0.1f,btnRect2.DOAnchorPos(belowScreenPos2, 0.6f).SetEase(Ease.InBack, 2)); // ȭ�� �Ʒ��� ������
        //startSequence.Join(btnRect3.DOAnchorPos(belowScreenPos3, 0.6f).SetEase(Ease.InBack, 2)); // ȭ�� �Ʒ��� ������

        //// �ִϸ��̼� �Ϸ� �� Pause UI ��Ȱ��ȭ
        //startSequence.OnComplete(() =>
        //{

        RectTransform rectTransform = mainBtn[0].GetComponent<RectTransform>();

        mainBtn[0].interactable = false;
        // ��ư�� Y�� �������� ���� �ִϸ��̼� (0.5�� ����)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
        .OnComplete(() =>
        {
            LoadingScene.LoadScene("03.GameScene");

        });
    }


    public void OptionButton() // ���� ��ư
    {
        RectTransform rectTransform = mainBtn[1].GetComponent<RectTransform>();

        mainBtn[1].interactable = false;
        OptionManager.Instance.EnterOption();

        // ��ư�� Y�� �������� ���� �ִϸ��̼� (0.5�� ����)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
            .OnComplete(() =>
            {
                mainBtn[1].interactable = true;
            });

        isOption = !isOption;
    }

    public void ExitButton()   // ���� ���� ��ư
    {
        RectTransform rectTransform = mainBtn[2].GetComponent<RectTransform>();

        mainBtn[2].interactable = false;

        // ��ư�� Y�� �������� ���� �ִϸ��̼� (0.5�� ����)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
            .OnComplete(() =>
            {
                mainBtn[2].interactable = true;
                Application.Quit();
            });

    }

    #endregion

    // ����� ������ �ҷ�����
    private void LoadData()
    {
        GameData data = SaveManager.Instance.LoadPlayerData();
        
        bestScore = data.score;
        money = data.money;

        bestScoreText.text = bestScore.ToString();
        moneyText.text = money.ToString();
    }

    // ��ư Ŭ�� �� ��鸮�� �ִϸ��̼� ����
    private void AnimateButton(Button button)
    {
        RectTransform rectTransform = button.GetComponent<RectTransform>();

        button.interactable = false;

        // ��ư�� Y�� �������� ���� �ִϸ��̼� (0.5�� ����)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
            .OnComplete(() =>
            {
                button.interactable = true;
            });
    }

    // ��� ��ư ��Ȱ��ȭ
    private void DisableButtons()
    {
        foreach (Button btn in mainBtn)
        {
            btn.interactable = false;
        }
    }

}
