using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
    public string filePath = "Assets/11.Achievement/Achievement.csv"; // CSV ���� ���
    [LabelText("���� ����")]
    public List<Achievement> achievements = new List<Achievement>();


    protected override void Start()
    {
        LoadAchievementsFromCSV();
    }

    void LoadAchievementsFromCSV()
    {
        // ���� �б�
        StreamReader sr = new StreamReader(filePath);
        bool firstLine = true;

        while (!sr.EndOfStream)
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

        sr.Close();
        Debug.Log("Achievements loaded: " + achievements.Count);
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

    // ���� ��Ȳ�� CSV ���Ͽ� �����ϴ� �޼ҵ�
    void SaveAchievementsToCSV()
    {
        using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
        {
            // ù �ٿ� ��� �ۼ�
            sw.WriteLine("AchievementName,Description,Goal,CurrentProgress,IsCompleted");

            // �������� ������ ����
            foreach (var achievement in achievements)
            {
                string line = achievement.achievementName + "," +
                              achievement.description + "," +
                              achievement.goal + "," +
                              achievement.currentProgress + "," +
                              achievement.isCompleted;
                sw.WriteLine(line);
            }
        }

        Debug.Log("Achievements saved to CSV.");
    }
}

