using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    [TabGroup("����", "������"), LabelText("���� ũ��")]
    public float curSize = 1.0f;
    [TabGroup("����", "������"), LabelText("�� �� �پ��� ũ��")]
    public float sizeDecreasePerSecond = 0.01f; // ���ʸ��� �پ��� ũ��

    [TabGroup("����", "�̵�"), LabelText("�̵� �ӵ�")]
    public float moveSpeed = 5.0f;
    [TabGroup("����", "�̵�"), LabelText("ȸ�� �ӵ�")]
    public float rotationSpeed = 5f;

    [TabGroup("����", "����"), LabelText("������ �� �ִ°�")]
    public bool canJump;
    [TabGroup("����", "����"), LabelText("���� ����")]
    public float jumpForce = 10.0f;
    [TabGroup("����", "����"), LabelText("�߷� ���ӵ�")]
    public float extraGravityForce = 9.81f; // �߰� �߷� ���ӵ�
    [TabGroup("����", "����"), LabelText("������ ƨ��� ��")]
    public float knockbackForce = 20f; // ���� �ε����� �� �з����� ��


}
