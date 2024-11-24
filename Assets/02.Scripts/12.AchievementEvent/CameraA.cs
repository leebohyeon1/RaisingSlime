using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraA : MonoBehaviour
{
    [SerializeField] private GameObject light;



    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            light.SetActive(true);
            StartCoroutine(lightOff());

            AchievementManager.Instance.UpdateAchievement
                (AchievementManager.Instance.achievements[7].achievementName, 1);
        }

    }

    private IEnumerator lightOff()
    {
        yield return new WaitForSeconds(0.2f);
        light.SetActive(false);
    }
}

