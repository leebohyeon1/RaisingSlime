using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; 

public class SaveManager : Singleton<SaveManager>
{
    protected override void Awake()
    {
        base.Awake();
    }

    public void SavePlayerData(GameData data)
    {
        string json = JsonUtility.ToJson(data);

        // 데이터를 저장할 경로 지정
        string path = Path.Combine(Application.persistentDataPath, "gameData.json");

        // 디스크에 파일 생성 및 저장
        File.WriteAllText(path, json);
    }

    public GameData LoadPlayerData()
    {
        string path = Path.Combine(Application.persistentDataPath, "gameData.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            GameData data = JsonUtility.FromJson<GameData>(json);
            return data;
        }
        else
        {
            // 기본값을 가진 새로운 GameData 생성
            GameData data = new GameData();
            SavePlayerData(data);
            return data;
        }
    }
}

[System.Serializable]
public class GameData
{
    public uint money;
    public uint score;

    public float bgmVolume;
    public float sfxVolume;
    public bool isBgmMuted;
    public bool isSfxMuted;

    // 기본 생성자 (매개변수가 없는 경우)
    public GameData()
    {
        // 기본값 설정
        this.money = 0;
        this.score = 0;
        this.bgmVolume = 1f; // 최대 볼륨
        this.sfxVolume = 1f;
        this.isBgmMuted = false;
        this.isSfxMuted = false;
    }

    // 모든 매개변수를 받는 생성자
    public GameData(uint money, uint score, float bgmVolume, float sfxVolume, bool isBgmMuted, bool isSfxMuted)
    {
        this.money = money;
        this.score = score;
        this.bgmVolume = bgmVolume;
        this.sfxVolume = sfxVolume;
        this.isBgmMuted = isBgmMuted;
        this.isSfxMuted = isSfxMuted;
    }  
}