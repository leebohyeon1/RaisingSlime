using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [LabelText("메인 UI"), SerializeField]
    private GameObject mainUI;
    [LabelText("옵션 UI"), SerializeField]
    private GameObject optionUI;

    [LabelText("메인 버튼"), SerializeField]
    private Button[] mainBtn;
    [LabelText("시작 누른 후 버튼"), SerializeField]
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

    public void StartButton()  // 게임 시작 버튼
    {
        foreach(Button btn in mainBtn)
        {
            btn.interactable = false; //버튼 비 동기화
        }

        startPushBtn.gameObject.SetActive(true);

        /*
        RectTransform btnRect1 = mainBtn[0].GetComponent<RectTransform>();
        RectTransform btnRect2 = mainBtn[1].GetComponent<RectTransform>();
        RectTransform btnRect3 = mainBtn[2].GetComponent<RectTransform>();

        // UI가 화면 위로 올라간 후 아래로 떨어지는 애니메이션
        Vector2 belowScreenPos1 = new Vector2(btnRect1.anchoredPosition.x, -(Screen.height / 2 + btnRect1.rect.height)); // 화면 아래로 임의 위치
        Vector2 belowScreenPos2 = new Vector2(btnRect2.anchoredPosition.x, -(Screen.height / 2 + btnRect2.rect.height)); // 화면 아래로 임의 위치
        Vector2 belowScreenPos3 = new Vector2(btnRect3.anchoredPosition.x, -(Screen.height / 2 + btnRect3.rect.height)); // 화면 아래로 임의 위치

        // 애니메이션 시퀀스
        Sequence startSequence = DOTween.Sequence();

        // 화면 위로 살짝 올라갔다가 아래로 떨어지는 애니메이션
        startSequence.Append(btnRect1.DOAnchorPos(belowScreenPos1, 0.6f).SetEase(Ease.InBack, 2)); // 화면 아래로 떨어짐
        startSequence.Insert(0.1f,btnRect2.DOAnchorPos(belowScreenPos2, 0.6f).SetEase(Ease.InBack, 2)); // 화면 아래로 떨어짐
        startSequence.Join(btnRect3.DOAnchorPos(belowScreenPos3, 0.6f).SetEase(Ease.InBack, 2)); // 화면 아래로 떨어짐

        // 애니메이션 완료 후 Pause UI 비활성화
        startSequence.OnComplete(() =>
        {*/

        
        //SceneManager.LoadScene(2);

        /*});*/


    }

    public void ExitButton()   // 게임 종료 버튼
    {
        Application.Quit();
    }

    public void OptionButton() // 설정 버튼
    {
        OptionManager.Instance.EnterOption();    
    }
}
