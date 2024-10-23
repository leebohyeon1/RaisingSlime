using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
    public static OptionManager Instance { get; private set; } // 싱글턴 선언

    [SerializeField] private GameObject option;
    [SerializeField] private GameObject optionUI;
    [SerializeField] private GameObject exitBtn;

    [SerializeField] private Slider[] soundSliders;
    [SerializeField] private Button[] bgmButtons;
    [SerializeField] private Button[] sfxButtons;

    private Vector2 optionOriginalPos;
    private Vector2 btnOriginalPos;

    private bool isBgmMuted = false;
    private bool isSfxMuted = false;

    private float previousBgmVolume = 1f;
    private float previousSfxVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // RectTransform에서 처음 위치를 가져옴
            optionOriginalPos = optionUI.GetComponent<RectTransform>().anchoredPosition;
            btnOriginalPos = exitBtn.GetComponent<RectTransform>().anchoredPosition;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }

        LoadSoundSettings();
    }

    private void Start()
    {
        // BGM 버튼 이벤트 등록
        bgmButtons[0].onClick.AddListener(() => SetBgmVolume(0)); // 볼륨 증가
        bgmButtons[1].onClick.AddListener(() => SetBgmVolume(1)); // 볼륨 감소
        bgmButtons[2].onClick.AddListener(() => ToggleBgmMute()); // 음소거 토글

        // SFX 버튼 이벤트 등록
        sfxButtons[0].onClick.AddListener(() => SetSfxVolume(0)); // 볼륨 증가
        sfxButtons[1].onClick.AddListener(() => SetSfxVolume(1)); // 볼륨 감소
        sfxButtons[2].onClick.AddListener(() => ToggleSfxMute()); // 음소거 토글

    }

    public void ExitOption()
    {
        exitBtn.GetComponent<Button>().interactable = false;

        if (option.activeSelf)
        {
            RectTransform optionRect = optionUI.GetComponent<RectTransform>();
            RectTransform btnRect = exitBtn.GetComponent<RectTransform>();

            // 화면 아래 위치 계산 (공통 코드로 이동)
            Vector2 belowScreenPos = new Vector2(optionRect.anchoredPosition.x, -(Screen.height / 2 + optionRect.rect.height));
            Vector2 btnBelowScreenPos = new Vector2(btnRect.anchoredPosition.x, belowScreenPos.y);

            // 애니메이션 시퀀스
            Sequence exitOptionSequence = DOTween.Sequence();

            exitOptionSequence.Append(btnRect.DOAnchorPos(btnBelowScreenPos, 0.6f).SetEase(Ease.InBack, 0.5f));
            exitOptionSequence.Insert(0.1f, optionRect.DOAnchorPos(belowScreenPos, 0.6f).SetEase(Ease.InBack, 1.7f));

            // 애니메이션 완료 후, 원래 자리로 복귀하고 비활성화
            exitOptionSequence.OnComplete(() =>
            {
                // 위치를 즉시 원래 자리로 설정
                optionRect.anchoredPosition = optionOriginalPos;
                btnRect.anchoredPosition = btnOriginalPos;



                option.SetActive(false); // 옵션 UI 끄기

                if (GameManager.Instance)
                {
                    GameManager.Instance.SetOption();
                }

            });

            // 타임 스케일에 상관없이 애니메이션이 작동하도록 설정
            exitOptionSequence.SetUpdate(true);
        }
    }

    public void EnterOption()
    {
        exitBtn.GetComponent<Button>().interactable = true;
        if (!option.activeSelf)
        {
            RectTransform optionRect = optionUI.GetComponent<RectTransform>();
            RectTransform btnRect = exitBtn.GetComponent<RectTransform>();

            optionRect.anchoredPosition = optionOriginalPos;
            btnRect.anchoredPosition = btnOriginalPos;

            option.SetActive(true);

            Vector2 offScreenPos = new Vector2(optionRect.anchoredPosition.x, Screen.height / 2 + optionRect.rect.height); // 화면 위의 임의 위치
            Vector2 btnOffScreenPos = new Vector2(btnRect.anchoredPosition.x, Screen.height / 2 + btnRect.rect.height); // 화면 위의 임의 위치

            optionRect.anchoredPosition = offScreenPos;
            btnRect.anchoredPosition = btnOffScreenPos;

            Sequence enterOptionSequence = DOTween.Sequence();

            enterOptionSequence.Append(btnRect.DOAnchorPos(btnOriginalPos, 0.8f).SetEase(Ease.OutElastic, 1.2f, 0.6f));
            enterOptionSequence.Insert(0.1f, optionRect.DOAnchorPos(optionOriginalPos, 0.6f).SetEase(Ease.OutElastic, 1.2f, 0.6f));


            enterOptionSequence.SetUpdate(true);
        }
    }

    private void SetBgmVolume(int index)
    {
        // 음소거 상태 해제
        if (isBgmMuted)
        {
            isBgmMuted = false;           
        }

        float volumeChange = 0.1f;
        if (index == 1)
        {
            soundSliders[0].value = Mathf.Max(0, soundSliders[0].value - volumeChange);
        }
        else
        {
            soundSliders[0].value = Mathf.Min(1, soundSliders[0].value + volumeChange);
        }

        AudioManager.Instance.SetBgmVolume(soundSliders[0].value);
    }


    private void SetSfxVolume(int index)
    {
        // 음소거 상태 해제
        if (isSfxMuted)
        {
            isSfxMuted = false;
        }

        float volumeChange = 0.1f;
        if (index == 1)
        {
            soundSliders[1].value = Mathf.Max(0, soundSliders[1].value - volumeChange);
        }
        else
        {
            soundSliders[1].value = Mathf.Min(1, soundSliders[1].value + volumeChange);
        }

        AudioManager.Instance.SetSfxVolume(soundSliders[1].value);
    }

    private void ToggleBgmMute()
    {
        isBgmMuted = !isBgmMuted;

        AudioManager.Instance.MuteBGM(isBgmMuted);

        // 사운드 설정 저장
        SaveSoundSettings();
    }

    private void ToggleSfxMute()
    {
        isSfxMuted = !isSfxMuted;


        AudioManager.Instance.MuteSFX(isSfxMuted);

        // 사운드 설정 저장
        SaveSoundSettings();
    }


    private void LoadSoundSettings()
    {
        // 저장된 데이터 로드
        GameData data = SaveManager.Instance.LoadPlayerData();

        // 볼륨 및 음소거 상태 적용
        soundSliders[0].value = data.bgmVolume;
        soundSliders[1].value = data.sfxVolume;

        isBgmMuted = data.isBgmMuted;
        isSfxMuted = data.isSfxMuted;

        // AudioManager에 적용
        AudioManager.Instance.SetBgmVolume(data.bgmVolume);
        AudioManager.Instance.SetSfxVolume(data.sfxVolume);

        AudioManager.Instance.MuteBGM(isBgmMuted);
        AudioManager.Instance.MuteSFX(isSfxMuted);
    }

    private void SaveSoundSettings()
    {
        // 현재 데이터 로드
        GameData data = SaveManager.Instance.LoadPlayerData();

        // 사운드 설정 업데이트
        data.bgmVolume = soundSliders[0].value;
        data.sfxVolume = soundSliders[1].value;
        data.isBgmMuted = isBgmMuted;
        data.isSfxMuted = isSfxMuted;

        // 데이터 저장
        SaveManager.Instance.SavePlayerData(data);
    }
}
