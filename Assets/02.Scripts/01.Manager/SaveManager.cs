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

    protected override void Awake()
    {
        base.Awake();
    }

    public void SavePlayerData(GameData data)
    {
        string json = JsonUtility.ToJson(data);

        // 데이터를 암호화
        string encryptionKey = GenerateEncryptionKey(); // 고유 키 생성
        string encryptedJson = Encrypt(json, encryptionKey);

        // 데이터를 저장할 경로 지정
        string path = Path.Combine(Application.persistentDataPath, "gameData.json");

        // 디스크에 파일 생성 및 저장
        File.WriteAllText(path, encryptedJson);
    }

    public GameData LoadPlayerData()
    {
        string path = Path.Combine(Application.persistentDataPath, "gameData.json");
        if (File.Exists(path))
        {
            string encryptionKey = GenerateEncryptionKey(); // 고유 키 생성
            // 암호화된 데이터를 불러와 복호화
            string encryptedJson = File.ReadAllText(path);
            string json = Decrypt(encryptedJson, encryptionKey);

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

    private string Encrypt(string text, string key)
    {
        using (Aes aesAlg = Aes.Create())
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            Array.Resize(ref keyBytes, aesAlg.KeySize / 8); // 32바이트 키로 조정
            aesAlg.Key = keyBytes;
            aesAlg.GenerateIV();
            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var ms = new MemoryStream())
            {
                ms.Write(aesAlg.IV, 0, aesAlg.IV.Length); // IV 저장
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
        string baseKey = "your-base-key-32-byte-length"; // 32바이트 기본 키
        string deviceId = SystemInfo.deviceUniqueIdentifier; // 고유 장치 ID
        string combinedKey = baseKey + deviceId;

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(combinedKey);
            byte[] hashBytes = sha256.ComputeHash(keyBytes);
            return Convert.ToBase64String(hashBytes).Substring(0, 32); // 첫 32바이트 사용
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

        this.openSkin[0] = true;
        for (int i = 1; i < this.openSkin.Length; i++) 
        {
            this.openSkin[i] = false;
        }

        this.curPlayerIndex = 0;
    }

    // 모든 매개변수를 받는 생성자
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