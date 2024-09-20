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
            isGrounded = false; // ���� �� ���� ���� ����
        }
    }

    // �ڵ� ũ�� ���� ���
    private void AutoDecreaseSize()
    {
        // �� �����Ӹ��� �ʴ� �پ��� ũ�⸦ ����
        float sizeDecreaseAmount = playerStat.sizeDecreasePerSecond * Time.deltaTime;

        // ���� ũ�⿡�� �پ�� ũ�� ���
        Vector3 newScale = transform.localScale - new Vector3(sizeDecreaseAmount, sizeDecreaseAmount, sizeDecreaseAmount);

        // ũ�Ⱑ 0 ���Ϸ� �پ���� �ʵ��� ���� (�ּ� ũ�� ����)
        if (newScale.x > 0.8f && newScale.y > 0.8f && newScale.z > 0.8f)
        {
            transform.localScale = newScale;
            playerStat.curSize = transform.localScale.x;  // ���� ũ�� ������Ʈ
        }
        else
        {
            Debug.Log("Die");
            Destroy(gameObject);
        }
    }

    // ���� ����ϴ� ���
    private void CompareSize(GameObject eatAble)
    {

        EatAbleObjectBase eatAbleObjectBase = eatAble.GetComponentInParent<EatAbleObjectBase>();

        // �����Ӱ� ������Ʈ�� size ���� ��
        if (eatAbleObjectBase.size < playerStat.curSize)
        {
            // �ڽź��� ����� ������ �Դ´�.
            Eat(eatAbleObjectBase);
        }
        else
        {
            if(eatAble.GetComponentInParent<EnemyBase>()) // �ڽ� ���� ����� ũ��, ���� ���
            {
                TakeDamage(eatAble.GetComponentInParent<EnemyBase>().collisionDamage);
            }
        }
    }

    private void Eat(EatAbleObjectBase eatAbleObjectBase) 
    {

        // �������� ũ�⸦ ������Ű��
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
            isGrounded = true; // ���� ������ �ٽ� ���� ����
        }

        // ����� �� �ִ� ������Ʈ�� �浹�ߴ��� Ȯ��
        if (collision.gameObject.GetComponentInParent<EatAbleObjectBase>())
        {
            CompareSize(collision.gameObject);
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
