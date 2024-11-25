using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneHenge : MonoBehaviour
{
    private bool isinPlayer = false;
    private float timer = 0f;

    private void Update()
    {
        if (isinPlayer)
        {
            timer += Time.deltaTime;
            if (timer > 3f)
            {
                AchievementManager.Instance.UpdateAchievement
                    (AchievementManager.Instance.achievements[6].achievementName, 1);
            }
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<Player>().SetInvincibility(true);
            isinPlayer = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().SetInvincibility(true);
            isinPlayer = false;
        }
    }
}
