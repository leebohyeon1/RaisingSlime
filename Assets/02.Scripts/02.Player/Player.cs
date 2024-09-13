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

    private bool isGrounded = true; // 땅에 있는지 확인하는 플래그

    private float hpDecreaseTimer = 0.0f;
    private float hpDecreaseInterval = 0.1f;

    void Start()
    {
        // 컴포넌트 초기화
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
            isGrounded = false; // 점프 후 땅에 있지 않음
        }
    }

    // 적을 흡수하는 기능
    private void AbsorbEnemy(GameObject enemy)
    {

        EatAbleObjectBase eatAbleObjectBase = enemy.GetComponentInParent<EatAbleObjectBase>();

        // 슬라임과 적의 size 변수 비교
        if (eatAbleObjectBase.size < playerStat.curSize)
        {
            // 슬라임의 크기를 증가시키기
            eatAbleObjectBase.Eaten(transform);

            transform.localScale += eatAbleObjectBase.SlimeIncreaseSize();
            playerStat.curSize = transform.localScale.x;
            playerStat.hp += eatAbleObjectBase.slimeRecoveryAmount;
        
        }
    }

    // 자동 체력 감소 기능
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
            isGrounded = true; // 땅에 닿으면 다시 점프 가능
        }

        // 흡수할 수 있는 오브젝트와 충돌했는지 확인
        if (collision.gameObject.GetComponentInParent<EatAbleObjectBase>())
        {
            AbsorbEnemy(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false; // 땅에 닿으면 다시 점프 가능
        }
    }

}
