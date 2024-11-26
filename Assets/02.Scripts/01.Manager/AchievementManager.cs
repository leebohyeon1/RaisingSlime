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
    [LabelText("����")]
    public string achievementName;
    [LabelText("����")]
    public string description;
    [LabelText("��ǥ")]
    public int goal;  // �������� �޼��� �ʿ��� �� (��: 10�� ����)
    [LabelText("���� ��Ȳ")]
    public int currentProgress;  // ���� ���� ��Ȳ
    [LabelText("�Ϸ�")]
    public bool isCompleted;
    [LabelText("������")]
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
    private string baseKey = "default-base-key-32-byte-long"; // �⺻ Ű (32����Ʈ�� ����)
    [LabelText("���� ����")]
    public List<Achievement> achievements { get; private set; } = new List<Achievement>();

    public Sprite[] sprites;

    // CSV ������ �ؽ� ���� �����ϱ� ���� Ű
    private const string CsvHashPlayerPrefsKey = "CsvHash";

    protected override void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "Achievement.csv");
        LoadAchievementsFromCSV();
    }

    // ���� Ű ���� �޼���: �⺻ Ű�� ���� ��ġ ID ����
    private string GenerateEncryptionKey()
    {
        string deviceId = SystemInfo.deviceUniqueIdentifier; // ��ġ ���� ID ��������
        string combinedKey = baseKey + deviceId;

        // 32����Ʈ ������ �ؽ� Ű ����
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(combinedKey);
            byte[] hashBytes = sha256.ComputeHash(keyBytes);
            return Convert.ToBase64String(hashBytes).Substring(0, 32); // ù 32����Ʈ ���
        }
    }

    void LoadAchievementsFromCSV()
    {
        string encryptionKey = GenerateEncryptionKey(); // ���� Ű ����
        string currentCsvHash = GetHash(csv.text); // ���� CSV�� �ؽ� ��

        // ������ ����� CSV �ؽ� �� ��������
        string savedCsvHash = PlayerPrefs.GetString(CsvHashPlayerPrefsKey, "");

        // CSV ������ ����Ǿ����� Ȯ��
        if (!File.Exists(filePath) || currentCsvHash != savedCsvHash)
        {
            // CSV ������ ���ų�, ����Ǿ��� ��� ������Ʈ
            CreateOrUpdateAchievementFile(currentCsvHash, encryptionKey);
        }

        // ���� �б�
        string encryptedData = File.ReadAllText(filePath);
        string csvData = Decrypt(encryptedData, encryptionKey); // ��ȣȭ

        achievements = ParseAchievementsFromCSV(csvData);

        // **��������Ʈ ���� �߰�**
        AssignSpritesToAchievements();

        Debug.Log("Achievements loaded: " + achievements.Count);
    }

    void AssignSpritesToAchievements()
    {
        for (int i = 0; i < achievements.Count; i++)
        {
            if (i < sprites.Length) // sprites �迭 ũ�� �̳��� ���� ����
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
        // ������ ����� ���뵵 �����͸� �ε� (���� ���)
        List<Achievement> existingAchievements = new List<Achievement>();
        if (File.Exists(filePath))
        {
            string encryptedData = File.ReadAllText(filePath);
            string csvData = Decrypt(encryptedData, encryptionKey); // ��ȣȭ
            existingAchievements = ParseAchievementsFromCSV(csvData);
        }

        // ���ο� CSV �����͸� �Ľ�
        List<Achievement> newAchievements = ParseAchievementsFromCSV(csv.text);

        // ���� ���൵�� �����ϸ鼭 ���ο� ���뵵 �߰�
        foreach (var newAchievement in newAchievements)
        {
            var existingAchievement = existingAchievements.Find(a => a.achievementName == newAchievement.achievementName);
            if (existingAchievement != null)
            {
                // ���� ���뵵�� ���� ��� ���൵ ����
                newAchievement.currentProgress = existingAchievement.currentProgress;
                newAchievement.isCompleted = existingAchievement.isCompleted;
            }
        }

        // ������Ʈ�� ���뵵 ����Ʈ ����
        achievements = newAchievements;

        // CSV�� ����
        SaveAchievementsToCSV();

        // ���ο� CSV �ؽ� �� ����
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
                    firstLine = false; // ù ��(���)�� ��ŵ
                    continue;
                }

                string[] data = line.Split(',');

                // ������ ��ȿ�� �˻�
                if (data.Length < 5)
                {
                    Debug.LogWarning("Invalid CSV line: " + line);
                    continue;
                }

                // Achievement Ŭ������ �°� ������ �Ҵ�
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

    // Ư�� ������ �����Ǹ� ���������� ������Ʈ
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
                // UI ���� �Ǵ� ���� ����
            }

         
            // CSV ���Ͽ� ����
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

        string encryptionKey = GenerateEncryptionKey(); // ���� Ű ����
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

    // ���ڿ��� �ؽ� ���� ����ϴ� �޼���
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
