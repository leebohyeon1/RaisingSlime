using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : Singleton<AudioManager>
{
    [Header("BGM Settings")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private List<AudioClip> bgmClips;

    [Header("SFX Settings")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private List<AudioClip> sfxClips;

    private Dictionary<string, AudioClip> bgmDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

    private bool isBgmMuted = false;
    private bool isSfxMuted = false;

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
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
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
            sfxSource.PlayOneShot(sfxDictionary[clipName]);
        }
        else
        {
            Debug.LogWarning($"SFX 클립 '{clipName}'을(를) 찾을 수 없습니다.");
        }
    }

    public void SetSfxVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }

    public void MuteSFX(bool mute)
    {
        isSfxMuted = mute;
        sfxSource.mute = mute;
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
