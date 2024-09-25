using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    [TabGroup("스탯", "사이즈"), LabelText("현재 크기")]
    public float curSize = 1.0f;
    [TabGroup("스탯", "사이즈"), LabelText("초 당 줄어드는 크기")]
    public float sizeDecreasePerSecond = 0.01f; // 매초마다 줄어드는 크기

    [TabGroup("스탯", "이동"), LabelText("이동 속도")]
    public float moveSpeed = 5.0f;
    [TabGroup("스탯", "이동"), LabelText("회전 속도")]
    public float rotationSpeed = 5f;

    [TabGroup("스탯", "점프"), LabelText("점프할 수 있는가")]
    public bool canJump;
    [TabGroup("스탯", "점프"), LabelText("점프 강도")]
    public float jumpForce = 10.0f;
    [TabGroup("스탯", "점프"), LabelText("중력 가속도")]
    public float extraGravityForce = 9.81f; // 추가 중력 가속도
    [TabGroup("스탯", "점프"), LabelText("적한테 튕기는 힘")]
    public float knockbackForce = 20f; // 적과 부딪혔을 때 밀려나는 힘


}
