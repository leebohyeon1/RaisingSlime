using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : Singleton<AudioManager>
{
    [Header("BGM Settings")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private List<AudioClip> bgmClips;

    [Header("SFX Settings")]
    [SerializeField] private int sfxSourceCount = 5; // SFX 풀링을 위한 오디오 소스 개수
    [SerializeField] private List<AudioClip> sfxClips;

    private List<AudioSource> sfxSources = new List<AudioSource>();

    private Dictionary<string, AudioClip> bgmDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

    private bool isBgmMuted = false;
    private bool isSfxMuted = false;
    private int currentSfxIndex = 0; // 현재 재생 중인 SFX 소스 인덱스

    protected override void Awake()
    {
        base.Awake();

        InitializeAudioSources();

        InitializeAudioClips();
    }

    protected override void Start()
    {
        base.Start();

    
    }

    private void InitializeAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.priority = 64;
        }

        for (int i = 0; i < sfxSourceCount; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.priority = 128; 
            sfxSources.Add(source);
        }
    }

    private void InitializeAudioClips()
    {
        foreach (var clip in bgmClips)
        {
            if (!bgmDictionary.ContainsKey(clip.name))
            {
                bgmDictionary.Add(clip.name, clip);
            }
        }

        foreach (var clip in sfxClips)
        {
            if (!sfxDictionary.ContainsKey(clip.name))
            {
                sfxDictionary.Add(clip.name, clip);
            }
        }
    }

    #region BGM Methods

    public bool CheckCurBGM(string clipName)
    {
        return bgmSource.clip == bgmDictionary[clipName]? true : false;
    }
    public void PlayBGM(string clipName, bool loop = true)
    {
        if (bgmDictionary.ContainsKey(clipName))
        {
            bgmSource.clip = bgmDictionary[clipName];
            bgmSource.loop = loop;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM 클립 '{clipName}'을(를) 찾을 수 없습니다.");
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void SetBgmVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }

    public void MuteBGM(bool mute)
    {
        isBgmMuted = mute;
        bgmSource.mute = mute;
    }

    public bool IsBgmMuted()
    {
        return isBgmMuted;
    }

    #endregion

    #region SFX Methods
    public void PlaySFX(string clipName)
    {
        if (sfxDictionary.ContainsKey(clipName))
        {
            AudioSource source = sfxSources[currentSfxIndex];
            source.PlayOneShot(sfxDictionary[clipName]);
            currentSfxIndex = (currentSfxIndex + 1) % sfxSources.Count; // 다음 인덱스로 이동
        }
        else
        {
            Debug.LogWarning($"SFX 클립 '{clipName}'을(를) 찾을 수 없습니다.");
        }
    }

    public void SetSfxVolume(float volume)
    {
        foreach (var source in sfxSources)
        {
            source.volume = Mathf.Clamp01(volume);
        }
    }

    public void MuteSFX(bool mute)
    {
        isSfxMuted = mute;
        foreach (var source in sfxSources)
        {
            source.mute = mute;
        }
    }

    public bool IsSfxMuted()
    {
        return isSfxMuted;
    }

    #endregion

    #region BgmFadeInOut
    public void FadeInBGM(string clipName, float duration)
    {
        StartCoroutine(FadeInCoroutine(clipName, duration));
    }

    private IEnumerator FadeInCoroutine(string clipName, float duration)
    {
        PlayBGM(clipName);
        bgmSource.volume = 0f;
        float timer = 0f;
        while (timer < duration)
        {
            bgmSource.volume = Mathf.Lerp(0f, 1f, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        bgmSource.volume = 1f;
    }

    public void FadeOutBGM(float duration)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = bgmSource.volume;
        float timer = 0f;
        while (timer < duration)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        bgmSource.volume = 0f;
        StopBGM();
    }
    #endregion
}
