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

        // �����͸� ������ ��� ����
        string path = Path.Combine(Application.persistentDataPath, "gameData.json");

        // ��ũ�� ���� ���� �� ����
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
            // �⺻���� ���� ���ο� GameData ����
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

    public bool[] openSkin = new bool[SkinManager.Instance.GetSkinCount()];

    public int curPlayerIndex;

    // �⺻ ������ (�Ű������� ���� ���)
    public GameData()
    {
        // �⺻�� ����
        this.money = 0;
        this.score = 0;
        this.bgmVolume = 1f; // �ִ� ����
        this.sfxVolume = 1f;
        this.isBgmMuted = false;
        this.isSfxMuted = false;

        this.openSkin[0] = true;
        for (int i = 1; i < this.openSkin.Length; i++) 
        {
            this.openSkin[i] = false;
        }

        this.curPlayerIndex = 0;
    }

    // ��� �Ű������� �޴� ������
    public GameData(uint money, uint score, float bgmVolume, float sfxVolume, bool isBgmMuted,
        bool isSfxMuted, bool[] openSkin, int playerIndex)
    {
        this.money = money;
        this.score = score;
        this.bgmVolume = bgmVolume;
        this.sfxVolume = sfxVolume;
        this.isBgmMuted = isBgmMuted;
        this.isSfxMuted = isSfxMuted;
        this.openSkin = openSkin;
        this.curPlayerIndex = playerIndex;
    }  
}