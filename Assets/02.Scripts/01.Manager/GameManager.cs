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

    [TabGroup("UI", "게임"), LabelText("점수 Text"), SerializeField]
    private TMP_Text scoreText;
    [TabGroup("UI", "게임"), LabelText("돈 Text"), SerializeField]
    private TMP_Text moneyText;

    [TabGroup("UI", "일시정지"), LabelText("일시정지 UI"), SerializeField]
    private GameObject pauseUI;

    //[TabGroup("UI", "옵션"), LabelText("옵션 UI"), SerializeField]
    //private GameObject optionUI;

    [TabGroup("UI", "게임오버"), LabelText("게임 오버 UI"), SerializeField]
    private GameObject gameOverUI;


    [TabGroup("UI", "게임오버"), LabelText("최고 점수 Text"), SerializeField]
    private TMP_Text bestScoreText;
    [TabGroup("UI", "게임오버"), LabelText("최종 점수 Text"), SerializeField]
    private TMP_Text totalScoreText;

    [TabGroup("UI", "게임오버"), LabelText("재시작 버튼"), SerializeField]
    private Button restartBtn;
    [TabGroup("UI", "게임오버"), LabelText("나가기 버튼"), SerializeField]
    private Button exitBtn;

    private float score = 0f;
    private uint money = 0;

    [BoxGroup("게임 상태"), LabelText("게임 오버"), SerializeField]
    private bool isGameOver = false;
    [BoxGroup("게임 상태"), LabelText("일시정지"), SerializeField]
    private bool isPause = false;
    [BoxGroup("게임 상태"), LabelText("설정"), SerializeField]
    private bool isOption = false;

    [BoxGroup("글로벌 볼륨"), SerializeField]
    private Volume globalVolume; // 글로벌 볼륨을 할당
    private DepthOfField depthOfField;

    // 일시정지 UI 기본 위치
    private Vector2 pauseOriginalPos;
    private bool canResume = true;

    // 게임오버 UI 기본 위치
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

        // Volume 프로파일에서 Depth of Field 컴포넌트를 가져오기
        if (globalVolume.profile.TryGet(out depthOfField))
        {
            // Bokeh 효과를 기본적으로 비활성화
            depthOfField.active = false;
        }

        InputManager.Instance.SwitchToActionMap("Player");

        // 원래 UI의 위치
        pauseOriginalPos = pauseUI.GetComponent<RectTransform>().anchoredPosition;
        gamOverOriginalPos = gameOverUI.GetComponent<RectTransform>().anchoredPosition;

        restartBtn.onClick.AddListener(() => RetryBtn());
        exitBtn.onClick.AddListener(() => ExitBtn());

        GameLogicManager.Instance.RegisterUpdatableObject(this);

        // 데이터 로드
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
    
    #region 점수

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

    #region 돈
  
    public void IncreaseMoney(uint increaseAmount = 1)
    {
        money += increaseAmount;
        moneyText.text = money.ToString();
    }

    #endregion

    #region 버튼
    public void PauseBtn() // 일시정지
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

            Vector2 offScreenPos = new Vector2(pauseRect.anchoredPosition.x, Screen.height / 2 + pauseRect.rect.height); // 화면 위의 임의 위치

            // 애니메이션 시퀀스
            Sequence pauseSequence = DOTween.Sequence();

            // 화면 위에서 원래 위치로 떨어지는 애니메이션 
            pauseRect.anchoredPosition = offScreenPos;
            pauseSequence.Append(pauseRect.DOAnchorPos(pauseOriginalPos, 0.8f).SetEase(Ease.OutElastic, 1.2f,0.6f));
            pauseSequence.InsertCallback(0.2f, () => { canResume = true; isPause = true; });

            // 타임 스케일에 상관없이 애니메이션이 작동하도록 설정
            pauseSequence.SetUpdate(true);

        }
        else if(canResume && isPause) 
        {
            canResume = false;
            // UI가 화면 위로 올라간 후 아래로 떨어지는 애니메이션
            Vector2 belowScreenPos = new Vector2(pauseRect.anchoredPosition.x, -(Screen.height / 2 + pauseRect.rect.height)); // 화면 아래로 임의 위치

            // 애니메이션 시퀀스
            Sequence resumeSequence = DOTween.Sequence();

            // 화면 위로 살짝 올라갔다가 아래로 떨어지는 애니메이션
            resumeSequence.Append(pauseRect.DOAnchorPos(belowScreenPos, 0.6f).SetEase(Ease.InBack)); // 화면 아래로 떨어짐

            // 애니메이션 완료 후 Pause UI 비활성화
            resumeSequence.OnComplete(() =>
            {          
                // 위치를 즉시 원래 자리로 설정
                pauseRect.anchoredPosition = pauseOriginalPos;
                
                pauseUI.SetActive(false);

                isPause = false;
                Time.timeScale = 1.0f;

                ToggleBokeh(false);


            });

            // 타임 스케일에 상관없이 애니메이션이 작동하도록 설정
            resumeSequence.SetUpdate(true);
        }
    }

    public void OptionBtn() // 옵션 
    {
        SetOption();

        OptionManager.Instance.EnterOption();
    }

    public void SetOption()
    {
        isOption = !isOption;
    } 

    public void RetryBtn()  // 게임 재시작
    {
        //LoadingScene.LoadScene("02.GameScene");
        SceneManager.LoadScene("03.GameScene");
    }

    public void ExitBtn()   // 메인화면으로 돌아감
    {
        SceneManager.LoadScene("02.MainScene");

        Time.timeScale = 1.0f;
    }
    #endregion

    public void GameOver() // 게임 오버됐을 때 작동하는 함수
    {
        isGameOver = true;
        gameOverUI.SetActive(true);

        ToggleBokeh(true); // 블러 온

        //버튼 제어 끄기
        restartBtn.interactable = false;
        exitBtn.interactable = false;

        //시간 정지
        Time.timeScale = 0f;

        GameData gameData = SaveManager.Instance.LoadPlayerData();
        bestScoreText.text = gameData.score.ToString();

        if (gameData.score < (uint)GetScore()) // 최고 점수 갱신 시
        {
            gameData.score = (uint)GetScore();
        }
        gameData.money = money;
        
        SaveManager.Instance.SavePlayerData(gameData);

        // UI가 화면 위에서 내려오는 애니메이션
        RectTransform gameOverRect = gameOverUI.GetComponent<RectTransform>();
        Vector2 offScreenPos = new Vector2(gameOverRect.anchoredPosition.x, Screen.height / 2 + gameOverRect.rect.height); // 화면 위의 임의 위치

        // 애니메이션 시퀀스
        Sequence gameOverSequence = DOTween.Sequence();

        // 화면 위에서 원래 위치로 떨어지는 애니메이션 (1초 동안)
        gameOverRect.anchoredPosition = offScreenPos;

        // 튕기는 듯한 애니메이션 효과
        gameOverSequence.Append(gameOverRect.DOAnchorPos(gamOverOriginalPos, 0.8f).SetEase(Ease.OutBounce, 10));

        // 0.1초 대기
        gameOverSequence.AppendInterval(0.1f).OnStart(() => totalScoreText.text = null);

        // 총점 카운터와 텍스트 흔들림 애니메이션 추가
        gameOverSequence.Append(totalScoreText.DOCounter(0, GetScore(), 1f));
        gameOverSequence.Join(totalScoreText.GetComponent<RectTransform>()
            .DOShakePosition(1f, new Vector3(0f, 2.5f, 0f), 20, 90, false, true));

        if (int.Parse(bestScoreText.text) < GetScore())
        {
            // 총점 카운터와 텍스트 흔들림 애니메이션 추가
            gameOverSequence.Append(bestScoreText.DOCounter(int.Parse(bestScoreText.text), GetScore(), 1f));
        }

        gameOverSequence.OnComplete(() =>
        {
            // 모든 UI 애니메이션 후 버튼 작동
            restartBtn.interactable = true;
            exitBtn.interactable = true;
        });

        // 타임 스케일에 상관없이 애니메이션이 작동하도록 설정
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

