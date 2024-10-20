using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 moveDirection; // �÷��̾� �̵� ����

    public Vector3 Move(Vector2 direction, float moveSpeed) // �÷��̾� �̵�
    {
        moveDirection.x = direction.x;
        moveDirection.z = direction.y;

        // 45�� ȸ�� (y�� ����)
        Quaternion rotation = Quaternion.Euler(0, -45, 0);
        Vector3 rotatedDirection = rotation * moveDirection;

        return rotatedDirection * moveSpeed;
    }

    public Vector3 Jump(float jumpForce)
    {
        return Vector3.up * jumpForce;
    }

}
