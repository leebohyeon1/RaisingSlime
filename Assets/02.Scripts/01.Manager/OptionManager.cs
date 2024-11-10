using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionManager : Singleton<OptionManager>
{
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

    [SerializeField] private Sprite[] soundIcon;
    [SerializeField] private Sprite[] muteIcon;
    

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        // RectTransform���� ó�� ��ġ�� ������
        optionOriginalPos = optionUI.GetComponent<RectTransform>().anchoredPosition;
        btnOriginalPos = exitBtn.GetComponent<RectTransform>().anchoredPosition;

        // BGM ��ư �̺�Ʈ ���
        bgmButtons[0].onClick.AddListener(() => SetBgmVolume(0)); // ���� ����
        bgmButtons[1].onClick.AddListener(() => SetBgmVolume(1)); // ���� ����
        bgmButtons[2].onClick.AddListener(() => ToggleBgmMute()); // ���Ұ� ���

        // SFX ��ư �̺�Ʈ ���
        sfxButtons[0].onClick.AddListener(() => SetSfxVolume(0)); // ���� ����
        sfxButtons[1].onClick.AddListener(() => SetSfxVolume(1)); // ���� ����
        sfxButtons[2].onClick.AddListener(() => ToggleSfxMute()); // ���Ұ� ���
       
        LoadSoundSettings();

        // �� �ε� �̺�Ʈ�� �޼��� ���
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void ExitOption()
    {
        AudioManager.Instance.PlaySFX("Btn");

        exitBtn.GetComponent<Button>().interactable = false;

        if (option.activeSelf)
        {
            RectTransform optionRect = optionUI.GetComponent<RectTransform>();
            RectTransform btnRect = exitBtn.GetComponent<RectTransform>();

            // ȭ�� �Ʒ� ��ġ ��� (���� �ڵ�� �̵�)
            Vector2 belowScreenPos = new Vector2(optionRect.anchoredPosition.x, -(Screen.height / 2 + optionRect.rect.height));
            Vector2 btnBelowScreenPos = new Vector2(btnRect.anchoredPosition.x, belowScreenPos.y);

            // �ִϸ��̼� ������
            Sequence exitOptionSequence = DOTween.Sequence();

            exitOptionSequence.Append(btnRect.DOAnchorPos(btnBelowScreenPos, 0.6f).SetEase(Ease.InBack, 0.5f));
            exitOptionSequence.Insert(0.1f, optionRect.DOAnchorPos(belowScreenPos, 0.6f).SetEase(Ease.InBack, 1.7f));

            // �ִϸ��̼� �Ϸ� ��, ���� �ڸ��� �����ϰ� ��Ȱ��ȭ
            exitOptionSequence.OnComplete(() =>
            {
                // ��ġ�� ��� ���� �ڸ��� ����
                optionRect.anchoredPosition = optionOriginalPos;
                btnRect.anchoredPosition = btnOriginalPos;



                option.SetActive(false); // �ɼ� UI ����

                if (GameManager.Instance)
                {
                    GameManager.Instance.SetOption();
                }

            });

            // Ÿ�� �����Ͽ� ������� �ִϸ��̼��� �۵��ϵ��� ����
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

            Vector2 offScreenPos = new Vector2(optionRect.anchoredPosition.x, Screen.height / 2 + optionRect.rect.height); // ȭ�� ���� ���� ��ġ
            Vector2 btnOffScreenPos = new Vector2(btnRect.anchoredPosition.x, Screen.height / 2 + btnRect.rect.height); // ȭ�� ���� ���� ��ġ

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

        AudioManager.Instance.PlaySFX("Btn");

        float volumeChange = 0.05f;
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
       
        AudioManager.Instance.PlaySFX("Btn");

        float volumeChange = 0.05f;
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
     
        AudioManager.Instance.PlaySFX("Btn");
        AudioManager.Instance.MuteBGM(isBgmMuted);

        if (isBgmMuted)
        {
            bgmButtons[2].GetComponent<Image>().sprite = muteIcon[0];
        }
        else
        {
            bgmButtons[2].GetComponent<Image>().sprite = soundIcon[0];
        }
        // ���� ���� ����
        SaveSoundSettings();
    }

    private void ToggleSfxMute()
    {
        isSfxMuted = !isSfxMuted;

        AudioManager.Instance.PlaySFX("Btn");
        AudioManager.Instance.MuteSFX(isSfxMuted);

        if (isSfxMuted)
        {
            sfxButtons[2].GetComponent<Image>().sprite = muteIcon[1];
        }
        else
        {
            sfxButtons[2].GetComponent<Image>().sprite = soundIcon[1];
        }

        // ���� ���� ����
        SaveSoundSettings();
    }


    private void LoadSoundSettings()
    {
        // ����� ������ �ε�
        GameData data = SaveManager.Instance.LoadPlayerData();

        // ���� �� ���Ұ� ���� ����
        soundSliders[0].value = data.bgmVolume;
        soundSliders[1].value = data.sfxVolume;

        isBgmMuted = data.isBgmMuted;
        isSfxMuted = data.isSfxMuted;


        if (isBgmMuted)
        {
            bgmButtons[2].GetComponent<Image>().sprite = muteIcon[0];
        }
        else
        {
            bgmButtons[2].GetComponent<Image>().sprite = soundIcon[0];
        }

        if (isSfxMuted)
        {
            sfxButtons[2].GetComponent<Image>().sprite = muteIcon[1];
        }
        else
        {
            sfxButtons[2].GetComponent<Image>().sprite = soundIcon[1];
        }
        // AudioManager�� ����
        AudioManager.Instance.SetBgmVolume(data.bgmVolume);
        AudioManager.Instance.SetSfxVolume(data.sfxVolume);

        AudioManager.Instance.MuteBGM(isBgmMuted);
        AudioManager.Instance.MuteSFX(isSfxMuted);
    }

    private void SaveSoundSettings()
    {
        // ���� ������ �ε�
        GameData data = SaveManager.Instance.LoadPlayerData();

        // ���� ���� ������Ʈ
        data.bgmVolume = soundSliders[0].value;
        data.sfxVolume = soundSliders[1].value;
        data.isBgmMuted = isBgmMuted;
        data.isSfxMuted = isSfxMuted;

        // ������ ����
        SaveManager.Instance.SavePlayerData(data);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���� �ε�� �� option�� ��Ȱ��ȭ
        if (option != null && option.activeSelf)
        {
            option.SetActive(false);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
