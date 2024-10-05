using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [TabGroup("UI", "����"), LabelText("���� Text"), SerializeField]
    private TMP_Text scoreText;

    [TabGroup("UI", "�Ͻ�����"), LabelText("�Ͻ����� UI"), SerializeField]
    private GameObject pauseUI;

    [TabGroup("UI", "�ɼ�"), LabelText("�ɼ� UI"), SerializeField]
    private GameObject optionUI;

    [TabGroup("UI","���ӿ���"), LabelText("���� ���� UI"), SerializeField]
    private GameObject gameOverUI;

    [TabGroup("UI", "���ӿ���"), LabelText("���� ���� Text"), SerializeField]
    private TMP_Text totalScoreText;

    [TabGroup("UI", "���ӿ���"), LabelText("����� ��ư"), SerializeField]
    private Button restartBtn;
    [TabGroup("UI", "���ӿ���"), LabelText("������ ��ư"), SerializeField]
    private Button exitBtn;

    private float score = 0f;

    [BoxGroup("���� ����"), LabelText("���� ����"), SerializeField]
    private bool isGameOver = false;
    [BoxGroup("���� ����"), LabelText("�Ͻ�����"), SerializeField]
    private bool isPause = false;
    [BoxGroup("���� ����"), LabelText("����"), SerializeField]
    private bool isOption = false;

    // �Ͻ����� UI �⺻ ��ġ
    private Vector2 pauseOriginalPos;
    private bool canResume = true;

    // ���ӿ��� UI �⺻ ��ġ
    private Vector2 gamOverOriginalPos;

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
            optionUI = OptionManager.Instance.optionUI;
        }

        // ���� UI�� ��ġ
        pauseOriginalPos = pauseUI.GetComponent<RectTransform>().anchoredPosition; 
        gamOverOriginalPos = gameOverUI.GetComponent<RectTransform>().anchoredPosition;

        restartBtn.onClick.AddListener(() => RetryBtn());
        exitBtn.onClick.AddListener(() => ExitBtn());

    }

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }

        UpdateScore();

        if(Input.GetKeyUp(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    #region ����

    public void UpdateScore()
    {
        score += Time.deltaTime;
        scoreText.text = GetScore().ToString("F0");
    }

    [Button]
    public void IncreaseScore(float plusScore)
    {
        score += plusScore;
    }
    #endregion


    #region ��ư
    public void PauseGame() // �Ͻ�����
    {
        if (isGameOver || isOption)
        {
            return;
        }

        RectTransform pauseRect = pauseUI.GetComponent<RectTransform>();

        if (!pauseUI.activeSelf && !isPause)
        {
            canResume = false;
            pauseRect.anchoredPosition = pauseOriginalPos;
            pauseUI.SetActive(true);
         
            Time.timeScale = 0.0f;
      

            Vector2 offScreenPos = new Vector2(pauseRect.anchoredPosition.x, Screen.height / 2 + pauseRect.rect.height); // ȭ�� ���� ���� ��ġ

            // �ִϸ��̼� ������
            Sequence pauseSequence = DOTween.Sequence();

            // ȭ�� ������ ���� ��ġ�� �������� �ִϸ��̼� 
            pauseRect.anchoredPosition = offScreenPos;
            pauseSequence.Append(pauseRect.DOAnchorPos(pauseOriginalPos, 0.8f).SetEase(Ease.OutElastic, 1.2f,0.6f));
            pauseSequence.InsertCallback(0.2f, () => { canResume = true; isPause = true; });

            // Ÿ�� �����Ͽ� ������� �ִϸ��̼��� �۵��ϵ��� ����
            pauseSequence.SetUpdate(true);
        }
        else if(canResume && isPause) 
        {
            canResume = false;
            // UI�� ȭ�� ���� �ö� �� �Ʒ��� �������� �ִϸ��̼�
            Vector2 belowScreenPos = new Vector2(pauseRect.anchoredPosition.x, -(Screen.height / 2 + pauseRect.rect.height)); // ȭ�� �Ʒ��� ���� ��ġ

            // �ִϸ��̼� ������
            Sequence resumeSequence = DOTween.Sequence();

            // ȭ�� ���� ��¦ �ö󰬴ٰ� �Ʒ��� �������� �ִϸ��̼�
            resumeSequence.Append(pauseRect.DOAnchorPos(belowScreenPos, 0.6f).SetEase(Ease.InBack)); // ȭ�� �Ʒ��� ������

            // �ִϸ��̼� �Ϸ� �� Pause UI ��Ȱ��ȭ
            resumeSequence.OnComplete(() =>
            {          
                // ��ġ�� ��� ���� �ڸ��� ����
                pauseRect.anchoredPosition = pauseOriginalPos;
                
                pauseUI.SetActive(false);

                isPause = false;
                Time.timeScale = 1.0f;
        

            });

            // Ÿ�� �����Ͽ� ������� �ִϸ��̼��� �۵��ϵ��� ����
            resumeSequence.SetUpdate(true);
        }
    }

    public void OptionBtn() // �ɼ� 
    {
        SetOption();

        OptionManager.Instance.EnterOption();
    }

    public void SetOption()
    {
        isOption = !isOption;
    }

    public void RetryBtn()  // ���� �����
    {
        SceneManager.LoadScene(2);
    }

    public void ExitBtn()   // ����ȭ������ ���ư�
    {
        SceneManager.LoadScene(1);

        Time.timeScale = 1.0f;
    }
    #endregion

    public void GameOver()
    {
        isGameOver = true;
        gameOverUI.SetActive(true);

        //��ư ���� ����
        restartBtn.interactable = false;
        exitBtn.interactable = false;

        Time.timeScale = 0f;

        // UI�� ȭ�� ������ �������� �ִϸ��̼�
        RectTransform gameOverRect = gameOverUI.GetComponent<RectTransform>();
        Vector2 offScreenPos = new Vector2(gameOverRect.anchoredPosition.x, Screen.height / 2 + gameOverRect.rect.height); // ȭ�� ���� ���� ��ġ

        // �ִϸ��̼� ������
        Sequence gameOverSequence = DOTween.Sequence();

        // ȭ�� ������ ���� ��ġ�� �������� �ִϸ��̼� (1�� ����)
        gameOverRect.anchoredPosition = offScreenPos;

        // ƨ��� ���� �ִϸ��̼� ȿ��
        gameOverSequence.Append(gameOverRect.DOAnchorPos(gamOverOriginalPos, 0.8f).SetEase(Ease.OutBounce, 10));

        // 0.1�� ���
        gameOverSequence.AppendInterval(0.1f).OnStart(() => totalScoreText.text = null);

        // ���� ī���Ϳ� �ؽ�Ʈ ��鸲 �ִϸ��̼� �߰�
        gameOverSequence.Append(totalScoreText.DOCounter(0, GetScore(), 1f));
        gameOverSequence.Join(totalScoreText.GetComponent<RectTransform>()
            .DOShakePosition(1f, new Vector3(0f, 2.5f, 0f), 20, 90, false, true));

        gameOverSequence.OnComplete(() =>
        {
            restartBtn.interactable = true;
            exitBtn.interactable = true;
        });

        // Ÿ�� �����Ͽ� ������� �ִϸ��̼��� �۵��ϵ��� ����
        gameOverSequence.SetUpdate(true);

        gameOverSequence.Play();

        
    }


    public int GetScore()
    {
        return (int)score;
    }
}
