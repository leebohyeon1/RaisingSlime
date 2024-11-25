using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour, IUpdateable
{
    public string skinName;

    private PlayerMovement playerMovement;
    private PlayerStat playerStat;

    private Rigidbody rb;

    private Vector3 movement;

    [BoxGroup("�� üũ"), LabelText("���� �ִ°�?"), SerializeField]
    private bool isGrounded = true; // ���� �ִ��� Ȯ���ϴ� �÷���
    [BoxGroup("�� üũ"), LabelText("�� üũ �Ÿ�"), SerializeField]
    private float groundCheckDistance = 0.1f; // Raycast�� �Ÿ�
    [BoxGroup("�� üũ"), LabelText("�� ���̾�"), SerializeField]
    private LayerMask groundLayer; // �� üũ�� ���� ���̾� ����ũ
    [BoxGroup("�� üũ"), LabelText("���� ��ƼŬ"), SerializeField]
    private GameObject smoke;

    private GameObject smokeParticle;

    public Material shadowMaterial;

    private Vector3 lastMovementDirection; // ���� �������� �̵� ����

    private bool isInvincibility = false;

    void Start()
    {
        // ������Ʈ �ʱ�ȭ
        playerMovement = GetComponent<PlayerMovement>();
        playerStat = GetComponent<PlayerStat>();

        rb = GetComponent<Rigidbody>();

        GameLogicManager.Instance.RegisterUpdatableObject(this);

        smokeParticle = Instantiate(smoke, transform);
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.GetGameOver())
        {
            return;
        }

        GroundCheck();

        HandleMovement();

        ApplyExtraGravity(); // ���߿� ���� �� �߷� ���ӵ� ����

        if (transform.position.y < -2f)
        {
            GameManager.Instance.GameOver();


            Destroy(gameObject);
        }
    }

    public virtual void OnUpdate(float dt)
    {
        if (GameManager.Instance.GetGameOver())
        {
            return;
        }

        if (GameManager.Instance.GetGameState())
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

        if (!isGrounded && !playerStat.canJump)
        {
            playerStat.canJump = true; // ���� �ʱ�ȭ
        }
        else if (isGrounded && !playerStat.canJump && rb.velocity.y < 0f)
        {
            playerStat.canJump = true; // ���� �ʱ�ȭ

        }
        // planeHeight ���� Raycast�� ���� ������ y��ǥ�� ����
        if (isGrounded && rb.velocity.magnitude > 0)
        {
            smokeParticle.SetActive(true);
            smokeParticle.transform.position = hit.point;


            // �̵� �ݴ� �������� smokeParticle�� ȸ�� ����
            Vector3 oppositeDirection = -movement.normalized; // �̵� �ݴ� ����
            if (oppositeDirection != Vector3.zero) // ������ 0�� �ƴ� ��츸 ȸ��
            {
                smokeParticle.transform.rotation = Quaternion.LookRotation(oppositeDirection);
            }
        }
        else
        {
            smokeParticle.SetActive(false);
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

        // �̵� ó��
        Vector3 newMovement = playerMovement.Move(InputManager.Instance.moveInput, playerStat.moveSpeed);

        // ���� �̵� ����� ���ο� �̵� ���� �� ���� ���
        float angleDifference = Vector3.Angle(lastMovementDirection, newMovement);

        // ���� ���� �̻� ���̳��� ���� ���� ����
        if (angleDifference > playerStat.angleDifference) // ��: 30�� �̻�
        {
            newMovement *= playerStat.inertiaFactor; // �̵� �ӵ� ����
        }

        // �̵� �ݿ�
        movement = newMovement;
        lastMovementDirection = movement.normalized; // ���� ���� ����

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
        if (isInvincibility)
        {
            return;
        }

        // �� �����Ӹ��� �ʴ� �پ��� ũ�⸦ ����
        float sizeDecreaseAmount = playerStat.sizeDecreasePerSecond * Time.deltaTime;

        // ���� ũ�⿡�� �پ�� ũ�� ���
        transform.localScale -= new Vector3(sizeDecreaseAmount, sizeDecreaseAmount, sizeDecreaseAmount);

        // ũ�Ⱑ 0 ���Ϸ� �پ���� �ʵ��� ���� (�ּ� ũ�� ����)
        if (transform.localScale.magnitude > 0.5f)
        {
            playerStat.curSize = transform.localScale.x;  // ���� ũ�� ������Ʈ
        }

        if (playerStat.curSize <= 0.5f)
        {
            GameManager.Instance.GameOver();

            Destroy(gameObject);
            // �״� �̺�Ʈ �߰�?
        }
    }

    // ���� ����ϴ� ���
    private void CompareSize(GameObject eatAble, bool isTrigger = false)
    {

        EatAbleObjectBase eatAbleObjectBase = eatAble.GetComponentInParent<EatAbleObjectBase>();


        // �����Ӱ� ������Ʈ�� size ���� ��
        if (eatAbleObjectBase.GetSize() < playerStat.curSize)
        {

            // �ڽź��� ����� ������ �Դ´�.
            Eat(eatAbleObjectBase);

            if (!isTrigger)
            {
                rb.velocity = movement / 3;
            }
        }
        else if (eatAble.GetComponentInParent<NPCBase>())
        {
            NPCBase npcBase = eatAble.GetComponentInParent<NPCBase>();
            if (npcBase.isEnemy) // �ڽ� ���� ����� ũ��, ���� ���
            {
                TakeDamage(npcBase.collisionDamage);

            }

            if (eatAble.GetComponent<PoliceCar>() != null)
            {
                AchievementManager.Instance.UpdateAchievement
                    (AchievementManager.Instance.achievements[9].achievementName, 1);

            }

            KnockbackFromEnemy(eatAble); // �� �ݴ� �������� ���ư���

        }
    }

    private void Eat(EatAbleObjectBase eatAbleObjectBase)
    {
        if (!GameManager.Instance.GetGameState())
        {
            GameManager.Instance.SetGameState();
        }

        AudioManager.Instance.PlaySFX("Slime");

        // ���� �� �ִ� ������Ʈ�� �ڽ� ������Ʈ�� �߰�
        eatAbleObjectBase.Eaten(transform);

        // �������� ũ�⸦ ����
        transform.localScale += eatAbleObjectBase.SlimeIncreaseSize();

        if (transform.localScale.x >= 13)
        {
            transform.localScale = new Vector3(13, 13, 13);
        }
        // ���� ����
        GameManager.Instance.IncreaseScore(eatAbleObjectBase.GetPlusScore());

        // ũ�⸦ üũ�ϴ� ���� �ʱ�ȭ
        playerStat.curSize = transform.localScale.x;
    }

    public void TakeDamage(float damage)
    {
        if (isInvincibility)
        {
            return;
        }

        transform.localScale -= new Vector3(damage, damage, damage);
        playerStat.curSize = transform.localScale.x;  // ���� ũ�� ������Ʈ

        if (playerStat.curSize <= 0.1f)
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
        Vector3 currentMoveDirection = (movement / 5);

        // ���� �ݴ� ����� ���� �̵� ������ �ջ��� �������� �з���
        Vector3 combinedKnockbackDirection = (knockbackDirection + Vector3.up).normalized;

        // �ش� �������� ���� ���� �о
        rb.AddForce(combinedKnockbackDirection * playerStat.knockbackForce, ForceMode.Impulse);
    }

    public Vector3 GetMovement()
    {
        return movement;
    }

    public void SetInvincibility(bool boolean)
    {
        isInvincibility = boolean;
    }


private void OnCollisionEnter(Collision collision)
    {
        // ����� �� �ִ� ������Ʈ�� �浹�ߴ��� Ȯ��
        if (collision.gameObject.GetComponentInParent<EatAbleObjectBase>())
        {
            CompareSize(collision.gameObject);

         
        }

        if(collision.gameObject.CompareTag("Sea"))
        {
            GameManager.Instance.GameOver();

            AchievementManager.Instance.UpdateAchievement
                 (AchievementManager.Instance.achievements[8].achievementName, 1);


            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        // ����� �� �ִ� ������Ʈ�� �浹�ߴ��� Ȯ��
        if (collider.GetComponentInParent<EatAbleObjectBase>())
        {
            CompareSize(collider.gameObject,true);
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
