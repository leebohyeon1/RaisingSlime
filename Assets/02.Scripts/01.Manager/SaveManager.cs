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
            try
            {
                string encryptionKey = GenerateEncryptionKey();
                string encryptedJson = File.ReadAllText(path);
                string json = Decrypt(encryptedJson, encryptionKey);

                GameData data = JsonUtility.FromJson<GameData>(json);

                // 버전 체크 및 변환
                if (data.version < version)
                {
                    data = MigrateData(data);
                }

                return data;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load game data: " + e.Message);

                // 데이터 손상 시 기본값 반환
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
        // 기존 데이터를 새 데이터 구조로 변환
        GameData newData = new GameData();

        // 기존 데이터의 값 복사
        newData.money = oldData.money;
        newData.score = oldData.score;
        newData.bgmVolume = oldData.bgmVolume;
        newData.sfxVolume = oldData.sfxVolume;
        newData.isBgmMuted = oldData.isBgmMuted;
        newData.isSfxMuted = oldData.isSfxMuted;

        // 새 구조에 맞게 추가 데이터 초기화
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
    public int version = 1; // 데이터 버전 추가

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
        this.version = 1; // 현재 데이터 버전
        // 기본값 설정
        this.money = 0;
        this.score = 0;
        this.bgmVolume = 0.5f; // 최대 볼륨
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

    // 모든 매개변수를 받는 생성자
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