using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (isGrounded)
        {
            //Destroy(gameObject);

            return;
        }

        EatAbleObjectBase eatAbleObjectBase = enemy.GetComponentInParent<EatAbleObjectBase>();

        // �����Ӱ� ���� size ���� ��
        if (eatAbleObjectBase.size < playerStat.curSize)
        {
            // �������� ũ�⸦ ������Ű��
            eatAbleObjectBase.GetEaten();
            transform.localScale += eatAbleObjectBase.SlimeIncreaseSize();
            playerStat.curSize = transform.localScale.x;
            playerStat.hp += eatAbleObjectBase.slimeRecoveryAmount;
         
            PlaceEnemyInsideSlime(enemy);
        }
    }

    // ���� ������ ������ ���� ��ġ�� ��ġ�ϴ� ���
    private void PlaceEnemyInsideSlime(GameObject enemy)
    {
        Transform enemyParent = enemy.transform.parent;
        // �������� ũ�⿡ ���� ���� ���� ���
        Vector3 slimeSize = transform.localScale;

        // ������ ������ ������ ��ġ ��� (������ �߾��� �������� ��ġ)
        Vector3 randomPosition = new Vector3(
            Random.Range(-slimeSize.x / 20, slimeSize.x / 20),
            Random.Range(-slimeSize.y / 20, slimeSize.y / 20),
            Random.Range(-slimeSize.z / 20, slimeSize.z / 20)
        );

        // ���� ��ġ�� ������ ���η� �̵�
        enemyParent.transform.localScale /= 1.5f;
        enemyParent.transform.SetParent(transform); // ���� �������� �ڽ����� �����Ͽ� �Բ� �̵��ϵ��� ��
        enemyParent.transform.localPosition = randomPosition; // ���� ��ǥ�� ������ ���ο� ��ġ

        // ���� ��Ȱ��ȭ�ϰų� ���������� ������ ���� �ʵ��� ����
        enemy.GetComponent<Collider>().enabled = false; // �浹 ��Ȱ��ȭ
        if (enemy.GetComponent<Rigidbody>() != null)
        {
            enemy.GetComponent<Rigidbody>().isKinematic = true; // Rigidbody�� ������ ��Ȱ��ȭ
        }
    }

    // ���� ��Ҵ��� Ȯ���ϴ� �Լ�
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; // ���� ������ �ٽ� ���� ����
        }

        // ���� �浹�ߴ��� Ȯ��
        if (collision.gameObject.CompareTag("Enemy"))
        {
            AbsorbEnemy(collision.gameObject);
        }
    }

    // ���� ��Ҵ��� Ȯ���ϴ� �Լ�
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false; // ���� ������ �ٽ� ���� ����
        }
    }
}
