using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [LabelText("일시정지 UI"), SerializeField]
    private GameObject pauseUI;

    [LabelText("옵션 UI"), SerializeField]
    private GameObject optionUI;

    void Start()
    {
        InputManager.Instance.SwitchToActionMap("Player");

        if (optionUI == null)
        {
            optionUI = OptionManager.Instance.OptionUI;
        }
    }

    public void PauseGame()
    {
        if (!pauseUI.activeSelf)
        {
            pauseUI.SetActive(true);
            Time.timeScale = 0.0f;
        }
        else
        {
            Time.timeScale = 1.0f;
            pauseUI.SetActive(false);
        }
    }

    public void OptionBtn()
    {
        optionUI.SetActive(true);
        pauseUI.SetActive(false);
    }

    public void RetryGame()
    {
        SceneManager.LoadScene(1);

        Time.timeScale = 1.0f;
    }
}
