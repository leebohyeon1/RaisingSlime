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
    [TabGroup("UI"), LabelText("메인 UI"), SerializeField]
    private GameObject mainUI;
    //[LabelText("옵션 UI"), SerializeField]
    //private GameObject optionUI;

    [TabGroup("UI", "버튼"), LabelText("메인 버튼"), SerializeField]
    private Button[] mainBtn;

    [TabGroup("UI", "점수"), LabelText("최고 점수 text"), SerializeField]
    private TMP_Text bestScoreText;

    [TabGroup("UI", "돈"), LabelText("최고 점수 text"), SerializeField]
    private TMP_Text moneyText;


    [BoxGroup("저장 데이터"), LabelText("보유 돈"), SerializeField]
    private uint money;
    [BoxGroup("저장 데이터"), LabelText("최고 점수"), SerializeField]
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

        // 각 버튼에 클릭 이벤트와 애니메이션 추가
        //foreach (Button btn in mainBtn)
        //{
        //    btn.onClick.AddListener(() => AnimateButton(btn));
        //}
    }

    #region button
    public void StartButton()  // 게임 시작 버튼
    {
        DisableButtons();

        //RectTransform btnRect1 = mainBtn[0].GetComponent<RectTransform>();
        //RectTransform btnRect2 = mainBtn[1].GetComponent<RectTransform>();
        //RectTransform btnRect3 = mainBtn[2].GetComponent<RectTransform>();

        //// UI가 화면 위로 올라간 후 아래로 떨어지는 애니메이션
        //Vector2 belowScreenPos1 = new Vector2(btnRect1.anchoredPosition.x, -(Screen.height / 2 + btnRect1.rect.height)); // 화면 아래로 임의 위치
        //Vector2 belowScreenPos2 = new Vector2(btnRect2.anchoredPosition.x, -(Screen.height / 2 + btnRect2.rect.height)); // 화면 아래로 임의 위치
        //Vector2 belowScreenPos3 = new Vector2(btnRect3.anchoredPosition.x, -(Screen.height / 2 + btnRect3.rect.height)); // 화면 아래로 임의 위치

        //// 애니메이션 시퀀스
        //Sequence startSequence = DOTween.Sequence();

        //// 화면 위로 살짝 올라갔다가 아래로 떨어지는 애니메이션
        //startSequence.Append(btnRect1.DOAnchorPos(belowScreenPos1, 0.6f).SetEase(Ease.InBack, 2)); // 화면 아래로 떨어짐
        //startSequence.Insert(0.1f,btnRect2.DOAnchorPos(belowScreenPos2, 0.6f).SetEase(Ease.InBack, 2)); // 화면 아래로 떨어짐
        //startSequence.Join(btnRect3.DOAnchorPos(belowScreenPos3, 0.6f).SetEase(Ease.InBack, 2)); // 화면 아래로 떨어짐

        //// 애니메이션 완료 후 Pause UI 비활성화
        //startSequence.OnComplete(() =>
        //{

        RectTransform rectTransform = mainBtn[0].GetComponent<RectTransform>();

        mainBtn[0].interactable = false;
        // 버튼을 Y축 방향으로 흔드는 애니메이션 (0.5초 동안)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
        .OnComplete(() =>
        {
            LoadingScene.LoadScene("03.GameScene");

        });
    }


    public void OptionButton() // 설정 버튼
    {
        RectTransform rectTransform = mainBtn[1].GetComponent<RectTransform>();

        mainBtn[1].interactable = false;
        OptionManager.Instance.EnterOption();

        // 버튼을 Y축 방향으로 흔드는 애니메이션 (0.5초 동안)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
            .OnComplete(() =>
            {
                mainBtn[1].interactable = true;
            });

        isOption = !isOption;
    }

    public void ExitButton()   // 게임 종료 버튼
    {
        RectTransform rectTransform = mainBtn[2].GetComponent<RectTransform>();

        mainBtn[2].interactable = false;

        // 버튼을 Y축 방향으로 흔드는 애니메이션 (0.5초 동안)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
            .OnComplete(() =>
            {
                mainBtn[2].interactable = true;
                Application.Quit();
            });

    }

    #endregion

    // 저장된 데이터 불러오기
    private void LoadData()
    {
        GameData data = SaveManager.Instance.LoadPlayerData();
        
        bestScore = data.score;
        money = data.money;

        bestScoreText.text = bestScore.ToString();
        moneyText.text = money.ToString();
    }

    // 버튼 클릭 시 흔들리는 애니메이션 적용
    private void AnimateButton(Button button)
    {
        RectTransform rectTransform = button.GetComponent<RectTransform>();

        button.interactable = false;

        // 버튼을 Y축 방향으로 흔드는 애니메이션 (0.5초 동안)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
            .OnComplete(() =>
            {
                button.interactable = true;
            });
    }

    // 모든 버튼 비활성화
    private void DisableButtons()
    {
        foreach (Button btn in mainBtn)
        {
            btn.interactable = false;
        }
    }

}
