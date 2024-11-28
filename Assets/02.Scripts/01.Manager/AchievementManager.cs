using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
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
    [LabelText("보상")]
    public int compensation;
    [LabelText("보상")]
    public bool isReceipt;

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
    }

    void AssignSpritesToAchievements()
    {
        Sprite defaultSprite = null; // 기본값 설정 가능

        for (int i = 0; i < achievements.Count; i++)
        {
            if (i < sprites.Length && sprites[i] != null)
            {
                achievements[i].Icon = sprites[i];
            }
            else
            {
                Debug.LogWarning($"No sprite available for achievement: {achievements[i].achievementName}. Assigning default sprite.");
                achievements[i].Icon = defaultSprite; // 기본값 할당
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
                newAchievement.isReceipt = existingAchievement.isReceipt;
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
            string headerLine = sr.ReadLine(); // 헤더 읽기
            if (headerLine == null)
            {
                Debug.LogError("CSV file is empty or invalid.");
                return parsedAchievements;
            }

            // 헤더를 기준으로 인덱스 매핑
            string[] headers = headerLine.Split(',');
            Dictionary<string, int> headerIndexMap = new Dictionary<string, int>();
            for (int i = 0; i < headers.Length; i++)
            {
                headerIndexMap[headers[i]] = i;
            }

            while (sr.Peek() != -1)
            {
                string line = sr.ReadLine();
                string[] data = line.Split(',');

                // 데이터가 헤더 개수보다 부족하면 기본값 추가
                if (data.Length < headers.Length)
                {
                    Array.Resize(ref data, headers.Length); // 데이터 배열 크기 확장
                }

                // Achievement 객체 생성
                Achievement achievement = new Achievement
                {
                    achievementName = headerIndexMap.ContainsKey("AchievementName") && !string.IsNullOrWhiteSpace(data[headerIndexMap["AchievementName"]]) ? data[headerIndexMap["AchievementName"]] : "Unknown",
                    description = headerIndexMap.ContainsKey("Description") && !string.IsNullOrWhiteSpace(data[headerIndexMap["Description"]]) ? data[headerIndexMap["Description"]] : "No Description",
                    goal = headerIndexMap.ContainsKey("Goal") && !string.IsNullOrWhiteSpace(data[headerIndexMap["Goal"]]) ? int.Parse(data[headerIndexMap["Goal"]]) : 0,
                    currentProgress = headerIndexMap.ContainsKey("CurrentProgress") && !string.IsNullOrWhiteSpace(data[headerIndexMap["CurrentProgress"]]) ? int.Parse(data[headerIndexMap["CurrentProgress"]]) : 0,
                    isCompleted = headerIndexMap.ContainsKey("IsCompleted") && !string.IsNullOrWhiteSpace(data[headerIndexMap["IsCompleted"]]) && data[headerIndexMap["IsCompleted"]].ToLower() == "true",
                    compensation = headerIndexMap.ContainsKey("Compensation") && !string.IsNullOrWhiteSpace(data[headerIndexMap["Compensation"]]) ? int.Parse(data[headerIndexMap["Compensation"]]) : 0,
                    isReceipt = headerIndexMap.ContainsKey("IsReceipt") && !string.IsNullOrWhiteSpace(data[headerIndexMap["IsReceipt"]]) && data[headerIndexMap["IsReceipt"]].ToLower() == "true",                 
                };

                parsedAchievements.Add(achievement);
            }
        }

        return parsedAchievements;
    }



    public void UpdateAchievement(string achievementName, int progress, bool isAcquisition = false)
    {
        Achievement achievement = achievements.Find(a => a.achievementName == achievementName);

        if (isAcquisition)
        {
            achievement.isReceipt = true;
        }

        if (!achievement.isCompleted)
        {
            achievement.currentProgress += progress;
            if (achievement.CheckCompletion())
            {
                achievement.isCompleted = true;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ShowAchievementBanner(achievementName);
                }

                Debug.Log($"Achievement completed: {achievement.achievementName}");

            
            }
        }

        // 완료된 도전 과제를 즉시 저장
        SaveAchievementsToCSV();
    }



    void SaveAchievementsToCSV()
    {
        StringBuilder csvData = new StringBuilder();
        csvData.AppendLine("AchievementName,Description,Goal,CurrentProgress,IsCompleted,Compensation,IsReceipt");

        foreach (var achievement in achievements)
        {
            csvData.AppendLine($"{achievement.achievementName},{achievement.description},{achievement.goal},{achievement.currentProgress},{achievement.isCompleted},{achievement.compensation},{achievement.isReceipt}");
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
