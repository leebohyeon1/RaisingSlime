using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    [LabelText("���� ũ��")]
    public float curSize = 1.0f;

    [LabelText("�� �� �پ��� ũ��")]
    public float sizeDecreasePerSecond = 0.01f; // ���ʸ��� �پ��� ũ��

    [LabelText("�̵� �ӵ�")]
    public float moveSpeed = 5.0f;

    [LabelText("���� ����")]
    public float jumpForce = 10.0f;

}
