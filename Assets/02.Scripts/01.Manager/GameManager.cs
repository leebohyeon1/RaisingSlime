using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [LabelText("�Ͻ����� UI"), SerializeField]
    private GameObject pauseUI;

    [LabelText("�ɼ� UI"), SerializeField]
    private GameObject optionUI;

    [LabelText("���� ���� UI"), SerializeField]
    private GameObject gameOverUI;

    [LabelText("���� UI"), SerializeField]
    private TMP_Text scoreText;
    [LabelText("���� ���� UI"), SerializeField]
    private TMP_Text totalScoreText;

    private float score = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Time.timeScale = 1.0f;
    }

    void Start()
    {
        InputManager.Instance.SwitchToActionMap("Player");

        if (optionUI == null)
        {
            optionUI = OptionManager.Instance.OptionUI;
        }
    }

    private void Update()
    {
        UpdateScore();
    }

    #region ����

    public void UpdateScore()
    {
        score += Time.deltaTime;
        scoreText.text = score.ToString("F0");
    }

    public void IncreaseScore(int plusScore)
    {
        score += plusScore;
    }
    #endregion


    #region ��ư
    public void PauseGame() // �Ͻ�����
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

    public void OptionBtn() // �ɼ� 
    {
        optionUI.SetActive(true);
        pauseUI.SetActive(false);
    }

    public void RetryBtn()  // ���� �����
    {
        SceneManager.LoadScene(1);
    }

    public void ExitBtn()   // ����ȭ������ ���ư�
    {
        SceneManager.LoadScene(0);

        Time.timeScale = 1.0f;
    }
    #endregion

    public void GameOver()
    {
        gameOverUI.SetActive(true);
        totalScoreText.text = scoreText.text;
        Time.timeScale = 0f;
    }

    public int GetScore()
    {
        return (int)score;
    }
}
