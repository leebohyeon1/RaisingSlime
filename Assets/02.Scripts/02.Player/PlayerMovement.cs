using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 moveDirection; // �÷��̾� �̵� ����

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public Vector3 Move(Vector2 direction, float moveSpeed) // �÷��̾� �̵�
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
