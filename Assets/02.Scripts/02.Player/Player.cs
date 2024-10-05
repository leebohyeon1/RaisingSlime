using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerStat playerStat;

    private Rigidbody rb;
    private Renderer slimeRenderer;

    private Vector3 movement;
    
    [BoxGroup("�� üũ"),LabelText("���� �ִ°�?"), SerializeField]
    private bool isGrounded = true; // ���� �ִ��� Ȯ���ϴ� �÷���
    [BoxGroup("�� üũ"), LabelText("�� üũ �Ÿ�"), SerializeField]
    private float groundCheckDistance = 0.1f; // Raycast�� �Ÿ�
    [BoxGroup("�� üũ"), LabelText("�� ���̾�"), SerializeField]
    private LayerMask groundLayer; // �� üũ�� ���� ���̾� ����ũ
    [BoxGroup("�� üũ"), LabelText("�� üũ ��ġ"), SerializeField]
    private Transform groundCheckPosition; // Raycast ���� ��ġ (�÷��̾� �߹�)

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


        AutoDecreaseSize();
        HandleJump();

       // CheckNavMesh(30f);
    }

    private void FixedUpdate()
    {
        GroundCheck();

        HandleMovement();

        ApplyExtraGravity(); // ���߿� ���� �� �߷� ���ӵ� ����
    }

    // ���� ��Ҵ��� Ȯ���ϴ� �Լ�
    private void GroundCheck()
    {
        // �÷��̾��� ���� ũ�⿡ ���� groundCheckDistance�� �������� ����
        float adjustedGroundCheckDistance = groundCheckDistance * transform.localScale.y;

        // �÷��̾� �߽ɿ��� �Ʒ��� Ray�� ���� ���� ��Ҵ��� Ȯ��
        isGrounded = Physics.Raycast(transform.position, Vector3.down, adjustedGroundCheckDistance, groundLayer);

        if (!isGrounded && !playerStat.canJump)
        {
            playerStat.canJump = true; // ���� �ʱ�ȭ
        }

        // ����׿� Ray �׸��� (��ġ�� ũ�⿡ ���� �� üũ�� �� �Ǵ��� Ȯ��)
        Debug.DrawRay(transform.position, Vector3.down * adjustedGroundCheckDistance, Color.blue);
    }

    private void HandleMovement()
    {
        if (!isGrounded)
        {
            rb.AddForce(movement * Time.deltaTime, ForceMode.VelocityChange);
            return;
        }

        // �̵� �� ���� ����Ͽ� rb.velocity�� ���� �̵� ó��
        movement = playerMovement.Move(InputManager.Instance.moveInput, playerStat.moveSpeed);

        // �÷��̾� �̵� �������� ȸ��
        /*if (movement != Vector3.zero) // �̵� ���� ��쿡�� ȸ��
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement); // �̵� ������ �ٶ󺸵��� ȸ�� ��ǥ ����
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * playerStat.rotationSpeed); // �ε巴�� ȸ��
        }*/

        rb.AddForce(movement * Time.deltaTime, ForceMode.VelocityChange);
    }

    private void HandleJump()
    {
        if (InputManager.Instance.jumpInput && isGrounded && playerStat.canJump) 
        {
            rb.AddForce(playerMovement.Jump(playerStat.jumpForce), ForceMode.Impulse);
            playerStat.canJump = false; // ���� �� ���� ���� ����
        }
    }

    // ���߿� ���� �� �߰� �߷� ���ӵ��� ����
    private void ApplyExtraGravity()
    {
        if (!isGrounded)
        {
            // ���߿� ���� �� �߷� ���ӵ��� �� ũ�� ����
            rb.AddForce(Vector3.down * playerStat.extraGravityForce, ForceMode.Acceleration);
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
            GameManager.Instance.GameOver();

            Destroy(gameObject);
            // �״� �̺�Ʈ �߰�?
        }
    }

    // ���� ����ϴ� ���
    private void CompareSize(GameObject eatAble)
    {

        EatAbleObjectBase eatAbleObjectBase = eatAble.GetComponentInParent<EatAbleObjectBase>();

        // �����Ӱ� ������Ʈ�� size ���� ��
        if (eatAbleObjectBase.GetSize() < playerStat.curSize)
        {
            // �ڽź��� ����� ������ �Դ´�.
            Eat(eatAbleObjectBase);
        }
        else
        {
            if(eatAble.GetComponentInParent<NPCBase>().isEnemy) // �ڽ� ���� ����� ũ��, ���� ���
            {
                TakeDamage(eatAble.GetComponentInParent<NPCBase>().collisionDamage);
                KnockbackFromEnemy(eatAble); // �� �ݴ� �������� ���ư���
            }
        }
    }

    private void Eat(EatAbleObjectBase eatAbleObjectBase) 
    {
        // ���� �� �ִ� ������Ʈ�� �ڽ� ������Ʈ�� �߰�
        eatAbleObjectBase.Eaten(transform);

        // �������� ũ�⸦ ����
        transform.localScale += eatAbleObjectBase.SlimeIncreaseSize();

        // ���� ����
        GameManager.Instance.IncreaseScore(eatAbleObjectBase.GetPlusScore());

        // ũ�⸦ üũ�ϴ� ���� �ʱ�ȭ
        playerStat.curSize = transform.localScale.x;
    }

    public void TakeDamage(float damage)
    {
        transform.localScale -= new Vector3(damage, damage, damage);
    }

    // ���� �浹���� �� �з����� ���
    private void KnockbackFromEnemy(GameObject enemy)
    {
        // ���� ��ġ���� �÷��̾� ��ġ�� ���ϴ� �ݴ� ���� ���� ���
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 knockbackDirection = (transform.position - enemyPosition).normalized;

        // ���� �̵� ������ �����ͼ� ����
        Vector3 currentMoveDirection = ( movement / 5) ;

        // ���� �ݴ� ����� ���� �̵� ������ �ջ��� �������� �з���
        Vector3 combinedKnockbackDirection = (knockbackDirection  + Vector3.up).normalized;

        // �ش� �������� ���� ���� �о
        rb.AddForce(combinedKnockbackDirection * playerStat.knockbackForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ����� �� �ִ� ������Ʈ�� �浹�ߴ��� Ȯ��
        if (collision.gameObject.GetComponentInParent<EatAbleObjectBase>())
        {
            CompareSize(collision.gameObject);
        }

    }

    public Vector3 GetMovement()
    {
        return movement;
    }
}
