using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    [LabelText("현재 크기")]
    public float curSize = 1.0f;

    [LabelText("초 당 줄어드는 크기")]
    public float sizeDecreasePerSecond = 0.01f; // 매초마다 줄어드는 크기

    [LabelText("이동 속도")]
    public float moveSpeed = 5.0f;

    [LabelText("점프 강도")]
    public float jumpForce = 10.0f;

}
