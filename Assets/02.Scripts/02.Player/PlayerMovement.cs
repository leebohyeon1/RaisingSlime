using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 moveDirection; // 플레이어 이동 방향

    public Vector3 Move(Vector2 direction, float moveSpeed) // 플레이어 이동
    {
        moveDirection.x = direction.x;
        moveDirection.z = direction.y;

        // 45도 회전 (y축 기준)
        Quaternion rotation = Quaternion.Euler(0, -45, 0);
        Vector3 rotatedDirection = rotation * moveDirection;

        return rotatedDirection * moveSpeed;
    }

    public Vector3 Jump(float jumpForce)
    {
        return Vector3.up * jumpForce;
    }

}
