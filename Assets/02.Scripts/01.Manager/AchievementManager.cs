using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

    public bool CheckCompletion()
    {
        return currentProgress >= goal;
    }
}

public class AchievementManager : Singleton<AchievementManager>
{
    public string filePath = "Assets/11.Achievement/Achievement.csv"; // CSV 파일 경로
    [LabelText("도전 과제")]
    public List<Achievement> achievements = new List<Achievement>();


    protected override void Start()
    {
        LoadAchievementsFromCSV();
    }

    void LoadAchievementsFromCSV()
    {
        // 파일 읽기
        StreamReader sr = new StreamReader(filePath);
        bool firstLine = true;

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            if (firstLine)
            {
                firstLine = false; // 첫 줄(헤더)은 스킵
                continue;
            }

            string[] data = line.Split(',');

            // Achievement 클래스에 맞게 데이터 할당
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
                Debug.Log("Achievement completed: " + achievement.achievementName);
                // UI 갱신 또는 보상 지급
            }

            // CSV 파일에 저장
            SaveAchievementsToCSV();
        }
    }

    // 진행 상황을 CSV 파일에 저장하는 메소드
    void SaveAchievementsToCSV()
    {
        using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
        {
            // 첫 줄에 헤더 작성
            sw.WriteLine("AchievementName,Description,Goal,CurrentProgress,IsCompleted");

            // 도전과제 데이터 저장
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

