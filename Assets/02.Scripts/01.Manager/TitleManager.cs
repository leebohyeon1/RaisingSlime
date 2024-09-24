using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [LabelText("타이틀 UI"), SerializeField]
    private GameObject titleUI;
    [LabelText("메인 UI"), SerializeField]
    private GameObject mainUI;
    [LabelText("옵션 UI"), SerializeField]
    private GameObject optionUI;

    void Start()
    {
        InputManager.Instance.SwitchToActionMap("Title"); // InputManager 액션 맵 Title로 교체

        if (optionUI == null)
        {
            optionUI = OptionManager.Instance.OptionUI;
        }

        titleUI.SetActive(true);
        mainUI.SetActive(false);
        optionUI.SetActive(false);
    }

    void Update()
    {
        InputKey();
    }

    private void InputKey()
    {
        if ( titleUI.activeSelf && InputManager.Instance.anyKeyInput)
        {
            titleUI.SetActive(false);
            mainUI.SetActive(true);
        }
    }

    public void StartButton()  // 게임 시작 버튼
    {
        SceneManager.LoadScene(1);
    }

    public void ExitButton()   // 게임 종료 버튼
    {
        Application.Quit();
    }

    public void OptionButton() // 설정 버튼
    {
        if (!optionUI.activeSelf)
        {
            optionUI.SetActive(true);
        }
    }
}
