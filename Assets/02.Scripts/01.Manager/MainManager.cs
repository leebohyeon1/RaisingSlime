using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    [LabelText("메인 UI"), SerializeField]
    private GameObject mainUI;
    [LabelText("옵션 UI"), SerializeField]
    private GameObject optionUI;

    void Start()
    {
        if (optionUI == null)
        {
            optionUI = OptionManager.Instance.OptionUI;
        }

        mainUI.SetActive(true);
        optionUI.SetActive(false);
    }

    void Update()
    {
        
    }

    public void StartButton()  // 게임 시작 버튼
    {
        SceneManager.LoadScene(2);
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
