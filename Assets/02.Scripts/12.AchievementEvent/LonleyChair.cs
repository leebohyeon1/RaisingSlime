using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LonleyChair : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            AchievementManager.Instance.UpdateAchievement
                (AchievementManager.Instance.achievements[11].achievementName, 1);
        }
    }
}
