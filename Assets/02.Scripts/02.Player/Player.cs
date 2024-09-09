using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerStat playerStat;

    private Rigidbody rb;
    private Renderer slimeRenderer;

    private bool isGrounded = true; // 땅에 있는지 확인하는 플래그


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
        if (isGrounded)
        {
            //Destroy(gameObject);

            return;
        }

        EatAbleObjectBase eatAbleObjectBase = enemy.GetComponentInParent<EatAbleObjectBase>();

        // 슬라임과 적의 size 변수 비교
        if (eatAbleObjectBase.size < playerStat.curSize)
        {
            // 슬라임의 크기를 증가시키기
            eatAbleObjectBase.GetEaten();
            transform.localScale += eatAbleObjectBase.SlimeIncreaseSize();
            playerStat.curSize = transform.localScale.x;
            playerStat.hp += eatAbleObjectBase.slimeRecoveryAmount;
         
            PlaceEnemyInsideSlime(enemy);
        }
    }

    // 적을 슬라임 내부의 랜덤 위치에 배치하는 기능
    private void PlaceEnemyInsideSlime(GameObject enemy)
    {
        Transform enemyParent = enemy.transform.parent;
        // 슬라임의 크기에 따른 내부 공간 계산
        Vector3 slimeSize = transform.localScale;

        // 슬라임 내부의 랜덤한 위치 계산 (슬라임 중앙을 기준으로 배치)
        Vector3 randomPosition = new Vector3(
            Random.Range(-slimeSize.x / 20, slimeSize.x / 20),
            Random.Range(-slimeSize.y / 20, slimeSize.y / 20),
            Random.Range(-slimeSize.z / 20, slimeSize.z / 20)
        );

        // 적의 위치를 슬라임 내부로 이동
        enemyParent.transform.localScale /= 1.5f;
        enemyParent.transform.SetParent(transform); // 적을 슬라임의 자식으로 설정하여 함께 이동하도록 함
        enemyParent.transform.localPosition = randomPosition; // 로컬 좌표로 슬라임 내부에 배치

        // 적을 비활성화하거나 물리적으로 영향을 받지 않도록 설정
        enemy.GetComponent<Collider>().enabled = false; // 충돌 비활성화
        if (enemy.GetComponent<Rigidbody>() != null)
        {
            enemy.GetComponent<Rigidbody>().isKinematic = true; // Rigidbody가 있으면 비활성화
        }
    }

    // 땅에 닿았는지 확인하는 함수
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; // 땅에 닿으면 다시 점프 가능
        }

        // 적과 충돌했는지 확인
        if (collision.gameObject.CompareTag("Enemy"))
        {
            AbsorbEnemy(collision.gameObject);
        }
    }

    // 땅에 닿았는지 확인하는 함수
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false; // 땅에 닿으면 다시 점프 가능
        }
    }
}
