using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : MonoBehaviour
{
    [SerializeField] private Transform spawnPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.position =  spawnPosition.position;

            AchievementManager.Instance.UpdateAchievement
                (AchievementManager.Instance.achievements[10].achievementName, 1);
        }
        else
        {
            other.GetComponent<EatAbleObjectBase>().Eaten(this.transform);
        }
    }
}
