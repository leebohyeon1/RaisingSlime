using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; 

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        // 싱글턴 선언
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    public void SavePlayerData(GameData data)
    {
        string json = JsonUtility.ToJson(data);

        // 데이터를 저장할 경로 지정
        string path = Path.Combine(Application.persistentDataPath, "gameData.json");

        // 디스크에 파일 생성 및 저장
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
            Debug.Log("저장된 데이터가 없어 새로 만들었습니다!");

            GameData data = new GameData(0, 0);
            SavePlayerData(data);
            return data;
        }
    }
}
