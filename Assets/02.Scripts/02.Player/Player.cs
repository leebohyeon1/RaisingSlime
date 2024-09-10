using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerStat playerStat;

    private Rigidbody rb;
    private Renderer slimeRenderer;

    private bool isGrounded = true; // ���� �ִ��� Ȯ���ϴ� �÷���

    private float hpDecreaseTimer = 0.0f;
    private float hpDecreaseInterval = 0.1f;

    void Start()
    {
        // ������Ʈ �ʱ�ȭ
        playerMovement = GetComponent<PlayerMovement>();      
        playerStat = GetComponent<PlayerStat>();

        rb = GetComponent<Rigidbody>();
        slimeRenderer = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        HandleMovement();
        HandleJump();

        AutoDecreaseHp();
    }

    private void HandleMovement()
    {
        if (!isGrounded)
        {
            return;
        }

        rb.velocity = playerMovement.Move(InputManager.Instance.moveInput, playerStat.moveSpeed); 
    }

    private void HandleJump()
    {
        if (InputManager.Instance.jumpInput && isGrounded) 
        {
            rb.AddForce(playerMovement.Jump(playerStat.jumpForce), ForceMode.Impulse);
            isGrounded = false; // ���� �� ���� ���� ����
        }
    }

    // ���� ����ϴ� ���
    private void AbsorbEnemy(GameObject enemy)
    {

        EatAbleObjectBase eatAbleObjectBase = enemy.GetComponentInParent<EatAbleObjectBase>();

        // �����Ӱ� ���� size ���� ��
        if (eatAbleObjectBase.size < playerStat.curSize)
        {
            // �������� ũ�⸦ ������Ű��
            eatAbleObjectBase.Eaten(transform);

            transform.localScale += eatAbleObjectBase.SlimeIncreaseSize();
            playerStat.curSize = transform.localScale.x;
            playerStat.hp += eatAbleObjectBase.slimeRecoveryAmount;
        
        }
    }

    // �ڵ� ü�� ���� ���
    private void AutoDecreaseHp()
    {
        hpDecreaseTimer += Time.deltaTime;

        if (hpDecreaseTimer > hpDecreaseInterval)
        {
            hpDecreaseTimer = 0.0f;
            playerStat.DecreaseHp();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; // ���� ������ �ٽ� ���� ����
        }

        // ����� �� �ִ� ������Ʈ�� �浹�ߴ��� Ȯ��
        if (collision.gameObject.GetComponentInParent<EatAbleObjectBase>())
        {
            AbsorbEnemy(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false; // ���� ������ �ٽ� ���� ����
        }
    }

}
