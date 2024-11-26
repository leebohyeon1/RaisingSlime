using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Achievement
{
    [LabelText("제목")]
    public string achievementName;
    [LabelText("설명")]
    public string description;
    [LabelText("목표")]
    public int goal;  // 도전과제 달성에 필요한 값 (예: 10번 점프)
    [LabelText("진행 상황")]
    public int currentProgress;  // 현재 진행 상황
    [LabelText("완료")]
    public bool isCompleted;
    [LabelText("아이콘")]
    public Sprite Icon;

    public bool CheckCompletion()
    {
        return currentProgress >= goal;
    }
}

public class AchievementManager : Singleton<AchievementManager>
{
    public TextAsset csv;
    private string filePath;
    private string baseKey = "default-base-key-32-byte-long"; // 기본 키 (32바이트로 설정)
    [LabelText("도전 과제")]
    public List<Achievement> achievements { get; private set; } = new List<Achievement>();

    public Sprite[] sprites;

    // CSV 파일의 해시 값을 저장하기 위한 키
    private const string CsvHashPlayerPrefsKey = "CsvHash";

    protected override void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "Achievement.csv");
        LoadAchievementsFromCSV();
    }

    // 고유 키 생성 메서드: 기본 키와 고유 장치 ID 조합
    private string GenerateEncryptionKey()
    {
        string deviceId = SystemInfo.deviceUniqueIdentifier; // 장치 고유 ID 가져오기
        string combinedKey = baseKey + deviceId;

        // 32바이트 길이의 해시 키 생성
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(combinedKey);
            byte[] hashBytes = sha256.ComputeHash(keyBytes);
            return Convert.ToBase64String(hashBytes).Substring(0, 32); // 첫 32바이트 사용
        }
    }

    void LoadAchievementsFromCSV()
    {
        string encryptionKey = GenerateEncryptionKey(); // 고유 키 생성
        string currentCsvHash = GetHash(csv.text); // 현재 CSV의 해시 값

        // 이전에 저장된 CSV 해시 값 가져오기
        string savedCsvHash = PlayerPrefs.GetString(CsvHashPlayerPrefsKey, "");

        // CSV 파일이 변경되었는지 확인
        if (!File.Exists(filePath) || currentCsvHash != savedCsvHash)
        {
            // CSV 파일이 없거나, 변경되었을 경우 업데이트
            CreateOrUpdateAchievementFile(currentCsvHash, encryptionKey);
        }

        // 파일 읽기
        string encryptedData = File.ReadAllText(filePath);
        string csvData = Decrypt(encryptedData, encryptionKey); // 복호화

        achievements = ParseAchievementsFromCSV(csvData);

        // **스프라이트 매핑 추가**
        AssignSpritesToAchievements();

        Debug.Log("Achievements loaded: " + achievements.Count);
    }

    void AssignSpritesToAchievements()
    {
        for (int i = 0; i < achievements.Count; i++)
        {
            if (i < sprites.Length) // sprites 배열 크기 이내일 때만 매핑
            {
                achievements[i].Icon = sprites[i];
            }
            else
            {
                Debug.LogWarning($"No sprite available for achievement: {achievements[i].achievementName}");
            }
        }
    }


    void CreateOrUpdateAchievementFile(string currentCsvHash, string encryptionKey)
    {
        // 기존에 저장된 성취도 데이터를 로드 (있을 경우)
        List<Achievement> existingAchievements = new List<Achievement>();
        if (File.Exists(filePath))
        {
            string encryptedData = File.ReadAllText(filePath);
            string csvData = Decrypt(encryptedData, encryptionKey); // 복호화
            existingAchievements = ParseAchievementsFromCSV(csvData);
        }

        // 새로운 CSV 데이터를 파싱
        List<Achievement> newAchievements = ParseAchievementsFromCSV(csv.text);

        // 기존 진행도를 유지하면서 새로운 성취도 추가
        foreach (var newAchievement in newAchievements)
        {
            var existingAchievement = existingAchievements.Find(a => a.achievementName == newAchievement.achievementName);
            if (existingAchievement != null)
            {
                // 기존 성취도가 있을 경우 진행도 유지
                newAchievement.currentProgress = existingAchievement.currentProgress;
                newAchievement.isCompleted = existingAchievement.isCompleted;
            }
        }

        // 업데이트된 성취도 리스트 저장
        achievements = newAchievements;

        // CSV로 저장
        SaveAchievementsToCSV();

        // 새로운 CSV 해시 값 저장
        PlayerPrefs.SetString(CsvHashPlayerPrefsKey, currentCsvHash);
        PlayerPrefs.Save();

        Debug.Log("Achievement file has been created or updated.");
    }

    List<Achievement> ParseAchievementsFromCSV(string csvData)
    {
        List<Achievement> parsedAchievements = new List<Achievement>();
        using (StringReader sr = new StringReader(csvData))
        {
            bool firstLine = true;

            while (sr.Peek() != -1)
            {
                string line = sr.ReadLine();
                if (firstLine)
                {
                    firstLine = false; // 첫 줄(헤더)은 스킵
                    continue;
                }

                string[] data = line.Split(',');

                // 데이터 유효성 검사
                if (data.Length < 5)
                {
                    Debug.LogWarning("Invalid CSV line: " + line);
                    continue;
                }

                // Achievement 클래스에 맞게 데이터 할당
                Achievement achievement = new Achievement();
                achievement.achievementName = data[0];
                achievement.description = data[1];
                achievement.goal = int.Parse(data[2]);
                achievement.currentProgress = int.Parse(data[3]);
                achievement.isCompleted = data[4].ToLower() == "true";

                parsedAchievements.Add(achievement);
            }
        }
        return parsedAchievements;
    }

    // 특정 조건이 만족되면 도전과제를 업데이트
    public void UpdateAchievement(string achievementName, int progress)
    {
        Achievement achievement = achievements.Find(a => a.achievementName == achievementName);
        if (achievement != null && !achievement.isCompleted)
        {
            achievement.currentProgress += progress;
            if (achievement.CheckCompletion())
            {
                achievement.isCompleted = true;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ShowAchievementBanner(achievementName);
                }

                Debug.Log("Achievement completed: " + achievement.achievementName);
                // UI 갱신 또는 보상 지급
            }

         
            // CSV 파일에 저장
            SaveAchievementsToCSV();
        }
    }

    void SaveAchievementsToCSV()
    {
        StringBuilder csvData = new StringBuilder();
        csvData.AppendLine("AchievementName,Description,Goal,CurrentProgress,IsCompleted");

        foreach (var achievement in achievements)
        {
            csvData.AppendLine($"{achievement.achievementName},{achievement.description},{achievement.goal},{achievement.currentProgress},{achievement.isCompleted}");
        }

        string encryptionKey = GenerateEncryptionKey(); // 고유 키 생성
        string encryptedData = Encrypt(csvData.ToString(), encryptionKey);
        File.WriteAllText(filePath, encryptedData);

        Debug.Log("Achievements saved to CSV.");
    }

    private string Encrypt(string text, string key)
    {
        using (Aes aesAlg = Aes.Create())
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            Array.Resize(ref keyBytes, aesAlg.KeySize / 8);
            aesAlg.Key = keyBytes;
            aesAlg.GenerateIV();
            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var ms = new MemoryStream())
            {
                ms.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(text);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private string Decrypt(string encryptedText, string key)
    {
        var fullCipher = Convert.FromBase64String(encryptedText);
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

    // 문자열의 해시 값을 계산하는 메서드
    private string GetHash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
