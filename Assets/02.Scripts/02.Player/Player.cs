using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour, IUpdateable
{
    private PlayerMovement playerMovement;
    private PlayerStat playerStat;

    private Rigidbody rb;

    private Vector3 movement;
    
    [BoxGroup("�� üũ"),LabelText("���� �ִ°�?"), SerializeField]
    private bool isGrounded = true; // ���� �ִ��� Ȯ���ϴ� �÷���
    [BoxGroup("�� üũ"), LabelText("�� üũ �Ÿ�"), SerializeField]
    private float groundCheckDistance = 0.1f; // Raycast�� �Ÿ�
    [BoxGroup("�� üũ"), LabelText("�� ���̾�"), SerializeField]
    private LayerMask groundLayer; // �� üũ�� ���� ���̾� ����ũ


    public Material shadowMaterial;
    public float planeHeight = 0.0f; // planeHeight ��

    void Start()
    {
        // ������Ʈ �ʱ�ȭ
        playerMovement = GetComponent<PlayerMovement>();      
        playerStat = GetComponent<PlayerStat>();

        rb = GetComponent<Rigidbody>();

        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    private void FixedUpdate()
    {
        GroundCheck();

        HandleMovement();

        ApplyExtraGravity(); // ���߿� ���� �� �߷� ���ӵ� ����
    }

    public virtual void OnUpdate(float dt)
    {
        if(GameManager.Instance.GetGameState())
        {
            AutoDecreaseSize();
        }
        
        HandleJump();
    }
    
    // ���� ��Ҵ��� Ȯ���ϴ� �Լ�
    private void GroundCheck()
    {
        // �÷��̾��� ���� ũ�⿡ ���� groundCheckDistance�� �������� ����
        float adjustedGroundCheckDistance = groundCheckDistance * transform.localScale.y;

        // �÷��̾� �߽ɿ��� �Ʒ��� Ray�� ���� ���� ��Ҵ��� Ȯ��
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, adjustedGroundCheckDistance, groundLayer);

        if (!isGrounded && !playerStat.canJump )
        {
            playerStat.canJump = true; // ���� �ʱ�ȭ
        }
        else if(isGrounded && !playerStat.canJump && rb.velocity.y < 0f)
        {
            playerStat.canJump = true; // ���� �ʱ�ȭ

        }
        // planeHeight ���� Raycast�� ���� ������ y��ǥ�� ����
        if (isGrounded)
        {
           // planeHeight = hit.point.y + 0.03f;  // ����ĳ��Ʈ�� ���� ��Ʈ ����Ʈ�� y ��ǥ�� ���
        }

      //  shadowMaterial.SetFloat("_PlaneHeight", planeHeight);


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
        transform.localScale -= new Vector3(sizeDecreaseAmount, sizeDecreaseAmount, sizeDecreaseAmount);

        // ũ�Ⱑ 0 ���Ϸ� �پ���� �ʵ��� ���� (�ּ� ũ�� ����)
        if (transform.localScale.magnitude > 0.4f)
        {
            playerStat.curSize = transform.localScale.x;  // ���� ũ�� ������Ʈ
        }
        
        if(playerStat.curSize <= 0.4f)
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
        else if(eatAble.GetComponentInParent<NPCBase>())
        {
            NPCBase npcBase = eatAble.GetComponentInParent<NPCBase>();
            if(npcBase.isEnemy) // �ڽ� ���� ����� ũ��, ���� ���
            {
                TakeDamage(npcBase.collisionDamage);
             
            }

               KnockbackFromEnemy(eatAble); // �� �ݴ� �������� ���ư���

        }
    }

    private void Eat(EatAbleObjectBase eatAbleObjectBase) 
    {
        if(!GameManager.Instance.GetGameState())
        {
            GameManager.Instance.SetGameState();
        }

        AudioManager.Instance.PlaySFX("Slime");
        
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
        playerStat.curSize = transform.localScale.x;  // ���� ũ�� ������Ʈ

        if (playerStat.curSize <= 0.2f)
        {
            GameManager.Instance.GameOver();

            Destroy(gameObject);
            // �״� �̺�Ʈ �߰�?
        }
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

    public Vector3 GetMovement()
    {
        return movement;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ����� �� �ִ� ������Ʈ�� �浹�ߴ��� Ȯ��
        if (collision.gameObject.GetComponentInParent<EatAbleObjectBase>())
        {
            CompareSize(collision.gameObject);
        }

    }

    private void OnTriggerEnter(Collider collider)
    {
        // ����� �� �ִ� ������Ʈ�� �浹�ߴ��� Ȯ��
        if (collider.GetComponentInParent<EatAbleObjectBase>())
        {
            CompareSize(collider.gameObject);
        }

    }

    void OnDestroy()
    {
        if (GameLogicManager.Instance != null)
        {
            GameLogicManager.Instance.DeregisterUpdatableObject(this);
        }
    }
    
}
