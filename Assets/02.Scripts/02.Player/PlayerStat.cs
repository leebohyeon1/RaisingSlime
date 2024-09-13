using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public float curSize = 1.0f;
    
    public float hp = 100.0f;
    public float decreaseHpAmount = 0.1f;


    public float moveSpeed = 5.0f;

    public float jumpForce = 10.0f;


    public void DecreaseHp(float decreaseHpAmount = 0f)
    {
        if (decreaseHpAmount == 0f)
        {
            hp -= this.decreaseHpAmount;
        }
        else
        {
            hp -= decreaseHpAmount;
        }

    }
}
