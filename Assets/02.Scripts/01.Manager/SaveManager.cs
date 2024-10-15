using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; 

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        // �̱��� ����
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

        // �����͸� ������ ��� ����
        string path = Path.Combine(Application.persistentDataPath, "gameData.json");

        // ��ũ�� ���� ���� �� ����
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
            Debug.Log("����� �����Ͱ� ���� ���� ��������ϴ�!");

            GameData data = new GameData(0, 0);
            SavePlayerData(data);
            return data;
        }
    }
}
