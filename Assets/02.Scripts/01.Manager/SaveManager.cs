using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System;

public class SaveManager : Singleton<SaveManager>
{
    public static int version = 2;

    protected override void Awake()
    {
        base.Awake();
    }

    public void SavePlayerData(GameData data)
    {
        string json = JsonUtility.ToJson(data);

        // �����͸� ��ȣȭ
        string encryptionKey = GenerateEncryptionKey(); // ���� Ű ����
        string encryptedJson = Encrypt(json, encryptionKey);

        // �����͸� ������ ��� ����
        string path = Path.Combine(Application.persistentDataPath, "gameData.json");

        // ��ũ�� ���� ���� �� ����
        File.WriteAllText(path, encryptedJson);
    }

    public GameData LoadPlayerData()
    {
        string path = Path.Combine(Application.persistentDataPath, "gameData.json");
        if (File.Exists(path))
        {
            try
            {
                string encryptionKey = GenerateEncryptionKey();
                string encryptedJson = File.ReadAllText(path);
                string json = Decrypt(encryptedJson, encryptionKey);

                GameData data = JsonUtility.FromJson<GameData>(json);

                // ���� üũ �� ��ȯ
                if (data.version < version)
                {
                    data = MigrateData(data);
                }

                return data;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load game data: " + e.Message);

                // ������ �ջ� �� �⺻�� ��ȯ
                GameData data = new GameData();
                data.version = version;
                SavePlayerData(data);
                return data;
            }
        }
        else
        {
            GameData data = new GameData();
            data.version = version;
            SavePlayerData(data);
            return data;
        }
    }


    private GameData MigrateData(GameData oldData)
    {
        // ���� �����͸� �� ������ ������ ��ȯ
        GameData newData = new GameData();

        // ���� �������� �� ����
        newData.money = oldData.money;
        newData.score = oldData.score;
        newData.bgmVolume = oldData.bgmVolume;
        newData.sfxVolume = oldData.sfxVolume;
        newData.isBgmMuted = oldData.isBgmMuted;
        newData.isSfxMuted = oldData.isSfxMuted;

        // �� ������ �°� �߰� ������ �ʱ�ȭ
        int skinCount = SkinManager.Instance.GetSkinCount();
        newData.openSkin = new bool[skinCount];
        Array.Copy(oldData.openSkin, newData.openSkin, Math.Min(oldData.openSkin.Length, skinCount));

        newData.curPlayerIndex = oldData.curPlayerIndex;

        return newData;
    }


    private string Encrypt(string text, string key)
    {
        using (Aes aesAlg = Aes.Create())
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            Array.Resize(ref keyBytes, aesAlg.KeySize / 8); // 32����Ʈ Ű�� ����
            aesAlg.Key = keyBytes;
            aesAlg.GenerateIV();
            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var ms = new MemoryStream())
            {
                ms.Write(aesAlg.IV, 0, aesAlg.IV.Length); // IV ����
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(text);
                    }
                }
                return System.Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private string Decrypt(string encryptedText, string key)
    {
        var fullCipher = System.Convert.FromBase64String(encryptedText);
        using (Aes aesAlg = Aes.Create())
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            Array.Resize(ref keyBytes, aesAlg.KeySize / 8);
            aesAlg.Key = keyBytes;

            var iv = fullCipher.Take(aesAlg.BlockSize / 8).ToArray();
            var cipher = fullCipher.Skip(aesAlg.BlockSize / 8).ToArray();

            aesAlg.IV = iv;
            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (var ms = new MemoryStream(cipher))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }

    private string GenerateEncryptionKey()
    {
        string baseKey = "your-base-key-32-byte-length"; // 32����Ʈ �⺻ Ű
        string deviceId = SystemInfo.deviceUniqueIdentifier; // ���� ��ġ ID
        string combinedKey = baseKey + deviceId;

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(combinedKey);
            byte[] hashBytes = sha256.ComputeHash(keyBytes);
            return Convert.ToBase64String(hashBytes).Substring(0, 32); // ù 32����Ʈ ���
        }
    }
}

[System.Serializable]
public class GameData
{
    public int version = 1; // ������ ���� �߰�

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
        this.version = 1; // ���� ������ ����
        // �⺻�� ����
        this.money = 0;
        this.score = 0;
        this.bgmVolume = 0.5f; // �ִ� ����
        this.sfxVolume = 0.5f;
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
    public GameData(int version, uint money, uint score, float bgmVolume, float sfxVolume, bool isBgmMuted,
        bool isSfxMuted, bool[] openSkin, int playerIndex)
    {
        this.version = version;
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