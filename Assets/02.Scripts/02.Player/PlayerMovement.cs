using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 moveDirection; // 플레이어 이동 방향

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public Vector3 Move(Vector2 direction, float moveSpeed) // 플레이어 이동
    {
        moveDirection.x = direction.x;
        moveDirection.z = direction.y;

        return moveDirection * moveSpeed;
    }

    public Vector3 Jump(float jumpForce)
    {
        return Vector3.up * jumpForce;
    }

}
