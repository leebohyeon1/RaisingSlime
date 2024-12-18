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
    [TabGroup("UI", "게임"), LabelText("튜토리얼 이미지 삭제예정"), SerializeField]
    private GameObject tutorialUI;
    private float tutorialTimer = 0;
    [TabGroup("UI", "게임"), LabelText("도전과제 배너"), SerializeField]
    private GameObject achievementBanner;
    private Vector2 bannerDefaultPos;
    [TabGroup("UI", "게임"), LabelText("별"), SerializeField]
    private GameObject[] starIccn;
    private int curStarCount = 0;


    [TabGroup("UI", "일시정지"), LabelText("일시정지 UI"), SerializeField]
    private GameObject pauseUI;

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

    [BoxGroup("게임 상태"), LabelText("게임 상태"), SerializeField]
    private bool isGame = false;
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
    private Vector2 gameOverOriginalPos;
    
    private bool isBGM2 = false;
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
        gameOverOriginalPos = gameOverUI.GetComponent<RectTransform>().anchoredPosition;
        bannerDefaultPos = achievementBanner.GetComponent<RectTransform>().anchoredPosition;


        restartBtn.onClick.AddListener(() => RetryBtn());
        exitBtn.onClick.AddListener(() => ExitBtn());

        GameLogicManager.Instance.RegisterUpdatableObject(this);

        // 데이터 로드
        GameData gameData = SaveManager.Instance.LoadPlayerData();
        money = gameData.money;
        moneyText.text = money.ToString();

        AudioManager.Instance.PlayBGM("GameBGM1");

        Cursor.lockState = CursorLockMode.None;

        for (int i = 0; i < starIccn.Length; i++)
        {
            starIccn[i].transform.localScale = Vector3.zero;
        }
    }

    public virtual void OnUpdate(float dt) 
    {
        if (isGameOver)
        {
            return;
        }

        tutorialTimer += Time.deltaTime;
        if(tutorialTimer > 5f)
        {
            tutorialUI.transform.GetChild(0).GetComponent<Image>().DOColor(new Color(1, 1, 1, 0), 0.5f);
            tutorialUI.transform.GetChild(1).GetComponent<Image>().DOColor(new Color(1, 1, 1, 0), 0.5f);
        }


        if (isGame)
        {
            UpdateScore();    
        }
        

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

        if(score > 10000 && !isBGM2)
        {
            isBGM2 = true;
            AudioManager.Instance.StopBGM();
            AudioManager.Instance.PlayBGM("GameBGM2");
        }
    }

    [Button]
    public void IncreaseScore(float plusScore)
    {
        score += plusScore;
    }

    public void UpdateStar()
    {
        starIccn[curStarCount].SetActive(true);
        starIccn[curStarCount].transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutElastic);
        curStarCount++;
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

        AudioManager.Instance.PlaySFX("Btn");
       
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

        AudioManager.Instance.PlaySFX("Btn");
        OptionManager.Instance.EnterOption();
    }

    public void SetOption()
    {
        isOption = !isOption;
    } 

    public void RetryBtn()  // 게임 재시작
    {
        AudioManager.Instance.PlaySFX("Btn");

        LoadingScene.LoadScene("03.GameScene");
    }

    public void ExitBtn()   // 메인화면으로 돌아감
    {
        AudioManager.Instance.PlaySFX("Btn");

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
        gameOverSequence.Append(gameOverRect.DOAnchorPos(gameOverOriginalPos, 0.8f).SetEase(Ease.OutBounce, 10));

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

    public void GameOverForAchievement()
    {
        isGameOver = true;
    }

    public bool GetGameOver()
    {
        return isGameOver;
    }

    public bool GetGameState()
    {
        return isGame;
    }

    public void SetGameState()
    {
        isGame = true;
        SpawnManager.Instance.StartSpawn();
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

    public void ShowAchievementBanner(string achievementName)
    {
        DOTween.Kill(achievementBanner);

        RectTransform bannerRect = achievementBanner.GetComponent<RectTransform>();
        achievementBanner.transform.GetChild(0).GetComponent<TMP_Text>().text = achievementName;
        // 시작 위치: 화면 아래로 이동
        Vector2 offScreenPos = new Vector2(bannerDefaultPos.x, -(Screen.height / 2 + bannerRect.rect.height));
        bannerRect.anchoredPosition = offScreenPos;

        // 화면 너비 계산
        float screenWidth = Screen.width;

        // 애니메이션 시퀀스 생성
        Sequence bannerSequence = DOTween.Sequence();

        // 화면 아래에서 기본 위치로 이동 (0.8초)
        bannerSequence.Append(bannerRect.DOAnchorPos(bannerDefaultPos, 0.5f).SetEase(Ease.OutBack))
            .OnStart(() => { achievementBanner.SetActive(true); });

        // 기본 위치에서 화면 밖으로 이동 (1.5초)
        Vector2 moveRightPos = new Vector2(bannerDefaultPos.x + screenWidth + bannerRect.rect.width, bannerDefaultPos.y);
        bannerSequence.Insert(1f,bannerRect.DOAnchorPos(moveRightPos, 0.5f).SetEase(Ease.Linear))
            .OnComplete(() => { achievementBanner.SetActive(false); });

        bannerSequence.SetUpdate(true);
        // 시퀀스 실행
        bannerSequence.Play();
    }



    void OnDestroy()
    {
        if (GameLogicManager.Instance != null)
        {
            GameLogicManager.Instance.DeregisterUpdatableObject(this);
        }
    }
}

