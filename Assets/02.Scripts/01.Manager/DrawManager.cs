using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DrawManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text moneyText;
    
    private uint haveMoney;

    private GameData gameData;
    
    void Start()
    {
        Init();
    }

    void Update()
    {
        
    }

    private void Init()
    {
        gameData = SaveManager.Instance.LoadPlayerData();
        haveMoney = gameData.money;
        moneyText.text = "Money: " + haveMoney;
    }
}
