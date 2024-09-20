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

        AutoDecreaseSize();
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

    // 자동 크기 감소 기능
    private void AutoDecreaseSize()
    {
        // 매 프레임마다 초당 줄어드는 크기를 적용
        float sizeDecreaseAmount = playerStat.sizeDecreasePerSecond * Time.deltaTime;

        // 현재 크기에서 줄어든 크기 계산
        Vector3 newScale = transform.localScale - new Vector3(sizeDecreaseAmount, sizeDecreaseAmount, sizeDecreaseAmount);

        // 크기가 0 이하로 줄어들지 않도록 제한 (최소 크기 설정)
        if (newScale.x > 0.8f && newScale.y > 0.8f && newScale.z > 0.8f)
        {
            transform.localScale = newScale;
            playerStat.curSize = transform.localScale.x;  // 현재 크기 업데이트
        }
        else
        {
            Debug.Log("Die");
            Destroy(gameObject);
        }
    }

    // 적을 흡수하는 기능
    private void CompareSize(GameObject eatAble)
    {

        EatAbleObjectBase eatAbleObjectBase = eatAble.GetComponentInParent<EatAbleObjectBase>();

        // 슬라임과 오브젝트의 size 변수 비교
        if (eatAbleObjectBase.size < playerStat.curSize)
        {
            // 자신보다 사이즈가 작으면 먹는다.
            Eat(eatAbleObjectBase);
        }
        else
        {
            if(eatAble.GetComponentInParent<EnemyBase>()) // 자신 보다 사이즈가 크고, 적일 경우
            {
                TakeDamage(eatAble.GetComponentInParent<EnemyBase>().collisionDamage);
            }
        }
    }

    private void Eat(EatAbleObjectBase eatAbleObjectBase) 
    {

        // 슬라임의 크기를 증가시키기
        eatAbleObjectBase.Eaten(transform);

        transform.localScale += eatAbleObjectBase.SlimeIncreaseSize();
        playerStat.curSize = transform.localScale.x;
    }

    public void TakeDamage(float damage)
    {
        transform.localScale -= new Vector3(damage, damage, damage);
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
            CompareSize(collision.gameObject);
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
