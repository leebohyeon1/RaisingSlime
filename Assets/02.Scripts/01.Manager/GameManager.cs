using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.Playables;

public class GameManager : MonoBehaviour, IUpdateable
{
    public static GameManager Instance { get; private set; }

    [TabGroup("UI", "����"), LabelText("���� Text"), SerializeField]
    private TMP_Text scoreText;
    [TabGroup("UI", "����"), LabelText("�� Text"), SerializeField]
    private TMP_Text moneyText;

    [TabGroup("UI", "�Ͻ�����"), LabelText("�Ͻ����� UI"), SerializeField]
    private GameObject pauseUI;

    //[TabGroup("UI", "�ɼ�"), LabelText("�ɼ� UI"), SerializeField]
    //private GameObject optionUI;

    [TabGroup("UI", "���ӿ���"), LabelText("���� ���� UI"), SerializeField]
    private GameObject gameOverUI;


    [TabGroup("UI", "���ӿ���"), LabelText("�ְ� ���� Text"), SerializeField]
    private TMP_Text bestScoreText;
    [TabGroup("UI", "���ӿ���"), LabelText("���� ���� Text"), SerializeField]
    private TMP_Text totalScoreText;

    [TabGroup("UI", "���ӿ���"), LabelText("����� ��ư"), SerializeField]
    private Button restartBtn;
    [TabGroup("UI", "���ӿ���"), LabelText("������ ��ư"), SerializeField]
    private Button exitBtn;

    private float score = 0f;
    private uint money = 0;

    [BoxGroup("���� ����"), LabelText("���� ����"), SerializeField]
    private bool isGameOver = false;
    [BoxGroup("���� ����"), LabelText("�Ͻ�����"), SerializeField]
    private bool isPause = false;
    [BoxGroup("���� ����"), LabelText("����"), SerializeField]
    private bool isOption = false;

    [BoxGroup("�۷ι� ����"), SerializeField]
    private Volume globalVolume; // �۷ι� ������ �Ҵ�
    private DepthOfField depthOfField;

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
        //if (optionUI == null)
        //{
        //    optionUI = OptionManager.Instance.optionUI;
        //}

        // Volume �������Ͽ��� Depth of Field ������Ʈ�� ��������
        if (globalVolume.profile.TryGet(out depthOfField))
        {
            // Bokeh ȿ���� �⺻������ ��Ȱ��ȭ
            depthOfField.active = false;
        }

        InputManager.Instance.SwitchToActionMap("Player");

        // ���� UI�� ��ġ
        pauseOriginalPos = pauseUI.GetComponent<RectTransform>().anchoredPosition;
        gamOverOriginalPos = gameOverUI.GetComponent<RectTransform>().anchoredPosition;

        restartBtn.onClick.AddListener(() => RetryBtn());
        exitBtn.onClick.AddListener(() => ExitBtn());

        GameLogicManager.Instance.RegisterUpdatableObject(this);

        // ������ �ε�
        GameData gameData = SaveManager.Instance.LoadPlayerData();
        money = gameData.money;
        moneyText.text = money.ToString();
    }

    public virtual void OnUpdate(float dt) 
    {
        if (isGameOver)
        {
            return;
        }

        UpdateScore();

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            PauseBtn();
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

    #region ��
  
    public void IncreaseMoney(uint increaseAmount = 1)
    {
        money += increaseAmount;
        moneyText.text = money.ToString();
    }

    #endregion

    #region ��ư
    public void PauseBtn() // �Ͻ�����
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

            ToggleBokeh(true);

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

                ToggleBokeh(false);


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
        //LoadingScene.LoadScene("02.GameScene");
        SceneManager.LoadScene("03.GameScene");
    }

    public void ExitBtn()   // ����ȭ������ ���ư�
    {
        SceneManager.LoadScene("02.MainScene");

        Time.timeScale = 1.0f;
    }
    #endregion

    public void GameOver() // ���� �������� �� �۵��ϴ� �Լ�
    {
        isGameOver = true;
        gameOverUI.SetActive(true);

        ToggleBokeh(true); // �� ��

        //��ư ���� ����
        restartBtn.interactable = false;
        exitBtn.interactable = false;

        //�ð� ����
        Time.timeScale = 0f;

        GameData gameData = SaveManager.Instance.LoadPlayerData();
        bestScoreText.text = gameData.score.ToString();

        if (gameData.score < (uint)GetScore()) // �ְ� ���� ���� ��
        {
            gameData.score = (uint)GetScore();
        }
        gameData.money = money;
        
        SaveManager.Instance.SavePlayerData(gameData);

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

        if (int.Parse(bestScoreText.text) < GetScore())
        {
            // ���� ī���Ϳ� �ؽ�Ʈ ��鸲 �ִϸ��̼� �߰�
            gameOverSequence.Append(bestScoreText.DOCounter(int.Parse(bestScoreText.text), GetScore(), 1f));
        }

        gameOverSequence.OnComplete(() =>
        {
            // ��� UI �ִϸ��̼� �� ��ư �۵�
            restartBtn.interactable = true;
            exitBtn.interactable = true;
        });

        // Ÿ�� �����Ͽ� ������� �ִϸ��̼��� �۵��ϵ��� ����
        gameOverSequence.SetUpdate(true);

        gameOverSequence.Play();

        
    }

    public void ToggleBokeh(bool enable)
    {
        if (depthOfField != null)
        {
            depthOfField.active = enable;
        }
    }

    public int GetScore()
    {
        return (int)score;
    }

    void OnDestroy()
    {
        if (GameLogicManager.Instance != null)
        {
            GameLogicManager.Instance.DeregisterUpdatableObject(this);
        }
    }
}

