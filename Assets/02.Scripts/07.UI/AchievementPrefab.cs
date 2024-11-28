using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPrefab : MonoBehaviour
{
    private MainManager mainManager;
    [SerializeField] private Image IconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image Icon;
    private Button compensationButton;
    private int compensationCoin;
    private bool isReceipt = false;
    [SerializeField] private GameObject compensationImage;
    
    //public Image unlockIcon;
    //public Image lockIcon;

    void Start()
    {
        compensationButton = GetComponent<Button>();
        compensationButton.onClick.AddListener(GetCompensation);
    }

    void Update()
    {
        
    }

    public void InitialPrefab(string title, string description,
     bool isCompleted, int compensation, bool isReceipt, Sprite sprite, MainManager mainManager)
    {
        this.mainManager = mainManager;
        // 버튼 컴포넌트 초기화
        if (compensationButton == null)
        {
            compensationButton = GetComponent<Button>();
            
        }

        titleText.text = title;
        descriptionText.text = description;
        compensationCoin = compensation;
        this.isReceipt = isReceipt;

        if (isCompleted)
        {
            if (Icon != null)
            {
                Icon.sprite = sprite;
            }

            compensationImage.SetActive(true);
            compensationImage.GetComponent<TMP_Text>().text = "+ " +  compensationCoin.ToString();


            if (!isReceipt)
            {
                compensationButton.interactable = true;
            }
            else
            {
                compensationImage.GetComponent<TMP_Text>().text = "수령 완료";
            }
        }
        else
        {
            compensationButton.interactable = false;
            compensationImage.SetActive(false);
        }
    }


    private void GetCompensation()
    {
        if (!isReceipt)
        {
            isReceipt = true;
            compensationButton.interactable = false;
            compensationImage.GetComponent<TMP_Text>().text = "수령 완료";


            GameData gameData = SaveManager.Instance.LoadPlayerData();
            gameData.money += (uint)compensationCoin;
            SaveManager.Instance.SavePlayerData(gameData);

            mainManager.UpdateGold(gameData);

            AchievementManager.Instance.UpdateAchievement(titleText.text, 0, true);
        }   
    }
}
