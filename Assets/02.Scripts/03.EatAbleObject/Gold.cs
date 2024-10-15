using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gold : EatAbleObjectBase
{
    public override void Eaten(Transform slimeTrans)
    {
        base.Eaten(slimeTrans);

        GameManager.Instance.IncreaseMoney();
    }
}
