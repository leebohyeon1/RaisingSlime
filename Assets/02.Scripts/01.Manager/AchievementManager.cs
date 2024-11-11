using Sirenix.OdinInspector;
using System;
using System.Collections;
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
        if (!File.Exists(filePath))
        {
            CreateDefaultAchievementFile();  // �⺻ ���� ����
        }

        // ���� �б�
        string encryptedData = File.ReadAllText(filePath);
        string encryptionKey = GenerateEncryptionKey(); // ���� Ű ����
        string csvData = Decrypt(encryptedData, encryptionKey); // ��ȣȭ

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

                // Achievement Ŭ������ �°� ������ �Ҵ�
                Achievement achievement = new Achievement();
                achievement.achievementName = data[0];
                achievement.description = data[1];
                achievement.goal = int.Parse(data[2]);
                achievement.currentProgress = int.Parse(data[3]);
                achievement.isCompleted = data[4].ToLower() == "true";

                achievements.Add(achievement);
            }
        }

        Debug.Log("Achievements loaded: " + achievements.Count);
    }

    void CreateDefaultAchievementFile()
    {
        string encryptionKey = GenerateEncryptionKey(); // ���� Ű ����
        string encryptedData = Encrypt(csv.text, encryptionKey);
        File.WriteAllText(filePath, encryptedData);

        Debug.Log("�⺻ Achievement ������ �����Ǿ����ϴ�: " + filePath);
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

}

