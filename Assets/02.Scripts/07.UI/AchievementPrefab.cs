using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPrefab : MonoBehaviour
{
    [SerializeField] private Image IconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image Icon;
    //public Image unlockIcon;
    //public Image lockIcon;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void InitialPrefab(string title, string description, bool isCompleted, Sprite sprite)
    {
        titleText.text = title;
        descriptionText.text = description;

        if (isCompleted)
        {
            Icon.sprite = sprite;
        }
    }
}
