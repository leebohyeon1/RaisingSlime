using Pathfinding;
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

    [BoxGroup("땅 체크"), LabelText("땅에 있는가?"), SerializeField]
    private bool isGrounded = true; // 땅에 있는지 확인하는 플래그
    [BoxGroup("땅 체크"), LabelText("땅 체크 거리"), SerializeField]
    private float groundCheckDistance = 0.1f; // Raycast의 거리
    [BoxGroup("땅 체크"), LabelText("땅 레이어"), SerializeField]
    private LayerMask groundLayer; // 땅 체크를 위한 레이어 마스크
    [BoxGroup("땅 체크"), LabelText("연기 파티클"), SerializeField]
    private GameObject smoke;

    private GameObject smokeParticle;

    public Material shadowMaterial;

    private Vector3 lastMovementDirection; // 이전 프레임의 이동 방향

    private bool isInvincibility = false;

    void Start()
    {
        // 컴포넌트 초기화
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

        ApplyExtraGravity(); // 공중에 있을 때 중력 가속도 적용

        if (transform.position.y < -5f)
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

    // 땅에 닿았는지 확인하는 함수
    private void GroundCheck()
    {

        // 플레이어의 현재 크기에 따라 groundCheckDistance를 동적으로 조정
        float adjustedGroundCheckDistance = groundCheckDistance * transform.localScale.y;

        // 플레이어 중심에서 아래로 Ray를 쏴서 땅에 닿았는지 확인
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, adjustedGroundCheckDistance, groundLayer) ||
                    Physics.Raycast(transform.position + (Vector3.right / 2.2f), Vector3.down, out RaycastHit hit1, adjustedGroundCheckDistance / 1.2f, groundLayer) ||
                    Physics.Raycast(transform.position - (Vector3.right / 2.2f), Vector3.down, out RaycastHit hit2, adjustedGroundCheckDistance / 1.2f, groundLayer) ||
                    Physics.Raycast(transform.position + (Vector3.forward / 2.2f), Vector3.down, out RaycastHit hit3, adjustedGroundCheckDistance / 1.2f, groundLayer) ||
                    Physics.Raycast(transform.position - (Vector3.forward / 2.2f), Vector3.down, out RaycastHit hit4, adjustedGroundCheckDistance / 1.2f, groundLayer);
        
        if (isGrounded && playerStat.canJump && rb.velocity.y < 0f)
        {
            playerStat.jumpCount = 1;
        }

        if (!isGrounded && !playerStat.canJump)
        {
            playerStat.canJump = true; // 점프 초기화
        }
        else if (isGrounded && !playerStat.canJump && rb.velocity.y < 0f)
        {
           
            playerStat.canJump = true; // 점프 초기화
        }
        // planeHeight 값을 Raycast로 구한 지점의 y좌표로 설정
        if (isGrounded && rb.velocity.magnitude > 0)
        {
            smokeParticle.SetActive(true);
            smokeParticle.transform.position = hit.point;


            // 이동 반대 방향으로 smokeParticle의 회전 설정
            Vector3 oppositeDirection = -movement.normalized; // 이동 반대 방향
            if (oppositeDirection != Vector3.zero) // 방향이 0이 아닌 경우만 회전
            {
                smokeParticle.transform.rotation = Quaternion.LookRotation(oppositeDirection);
            }
        }
        else
        {
            smokeParticle.SetActive(false);
        }

        //  shadowMaterial.SetFloat("_PlaneHeight", planeHeight);


        // 디버그용 Ray 그리기 (위치와 크기에 맞춰 땅 체크가 잘 되는지 확인)
        Debug.DrawRay(transform.position, Vector3.down * adjustedGroundCheckDistance, Color.blue);
    }

    private void HandleMovement()
    {
        if (!isGrounded)
        {
            rb.AddForce(movement * Time.deltaTime, ForceMode.VelocityChange);
            return;
        }

        // 이동 처리
        Vector3 newMovement = playerMovement.Move(InputManager.Instance.moveInput, playerStat.moveSpeed);

        // 이전 이동 방향과 새로운 이동 방향 간 각도 계산
        float angleDifference = Vector3.Angle(lastMovementDirection, newMovement);

        // 일정 각도 이상 차이나면 관성 감소 적용
        if (angleDifference > playerStat.angleDifference) // 예: 30도 이상
        {
            newMovement *= playerStat.inertiaFactor; // 이동 속도 감소
        }

        // 이동 반영
        movement = newMovement;
        lastMovementDirection = movement.normalized; // 현재 방향 저장

      
        rb.AddForce(movement * Time.deltaTime, ForceMode.VelocityChange);
    }

    private void HandleJump()
    {
        if (InputManager.Instance.jumpInput && playerStat.jumpCount == 1&& playerStat.canJump)
        {
            rb.AddForce(playerMovement.Jump(playerStat.jumpForce), ForceMode.Impulse);
            playerStat.canJump = false; // 점프 후 땅에 있지 않음
            playerStat.jumpCount = 0;
        }
    }

    // 공중에 있을 때 추가 중력 가속도를 적용
    private void ApplyExtraGravity()
    {
        if (!isGrounded)
        {
            // 공중에 있을 때 중력 가속도를 더 크게 적용
            rb.AddForce(Vector3.down * playerStat.extraGravityForce, ForceMode.Acceleration);
        }
    }

    // 자동 크기 감소 기능
    private void AutoDecreaseSize()
    {
        if (isInvincibility)
        {
            return;
        }

        // 매 프레임마다 초당 줄어드는 크기를 적용
        float sizeDecreaseAmount = playerStat.sizeDecreasePerSecond * Time.deltaTime;

        // 현재 크기에서 줄어든 크기 계산
        transform.localScale -= new Vector3(sizeDecreaseAmount, sizeDecreaseAmount, sizeDecreaseAmount);

        // 크기가 0 이하로 줄어들지 않도록 제한 (최소 크기 설정)
        if (transform.localScale.magnitude > 0.5f)
        {
            playerStat.curSize = transform.localScale.x;  // 현재 크기 업데이트
        }

        if (playerStat.curSize <= 0.5f)
        {
            GameManager.Instance.GameOver();

            Destroy(gameObject);
            // 죽는 이벤트 추가?
        }
    }

    // 적을 흡수하는 기능
    private void CompareSize(GameObject eatAble)
    {

        EatAbleObjectBase eatAbleObjectBase = eatAble.GetComponentInParent<EatAbleObjectBase>();


        // 슬라임과 오브젝트의 size 변수 비교
        if (eatAbleObjectBase.GetSize() < playerStat.curSize)
        {

            // 자신보다 사이즈가 작으면 먹는다.
            Eat(eatAbleObjectBase);
        }
        else if (eatAble.GetComponentInParent<NPCBase>())
        {
            NPCBase npcBase = eatAble.GetComponentInParent<NPCBase>();
            if (npcBase.isEnemy) // 자신 보다 사이즈가 크고, 적일 경우
            {
                TakeDamage(npcBase.collisionDamage);

            }

            if (eatAble.GetComponent<PoliceCar>() != null)
            {
                AchievementManager.Instance.UpdateAchievement
                    (AchievementManager.Instance.achievements[9].achievementName, 1);

            }

            KnockbackFromEnemy(eatAble); // 적 반대 방향으로 날아가기

        }
    }

    private void Eat(EatAbleObjectBase eatAbleObjectBase)
    {
        if (!GameManager.Instance.GetGameState())
        {
            GameManager.Instance.SetGameState();
        }

        AudioManager.Instance.PlaySFX("Slime");

        // 먹을 수 있는 오브젝트를 자식 오브젝트에 추가
        eatAbleObjectBase.Eaten(transform);

        // 슬라임의 크기를 증가
        transform.localScale += eatAbleObjectBase.SlimeIncreaseSize();

        if (transform.localScale.x >= 13)
        {
            transform.localScale = new Vector3(13, 13, 13);
        }
        // 점수 증가
        GameManager.Instance.IncreaseScore(eatAbleObjectBase.GetPlusScore());

        // 크기를 체크하는 변수 초기화
        playerStat.curSize = transform.localScale.x;
    }

    public void TakeDamage(float damage)
    {
        if (isInvincibility)
        {
            return;
        }

        transform.localScale -= new Vector3(damage, damage, damage);
        playerStat.curSize = transform.localScale.x;  // 현재 크기 업데이트

        if (playerStat.curSize <= 0.1f)
        {
            GameManager.Instance.GameOver();

            Destroy(gameObject);
            // 죽는 이벤트 추가?
        }
    }

    // 적과 충돌했을 때 밀려나는 기능
    private void KnockbackFromEnemy(GameObject enemy)
    {
        // 적의 위치에서 플레이어 위치로 향하는 반대 방향 벡터 계산
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 knockbackDirection = (transform.position - enemyPosition).normalized;

        // 현재 이동 방향을 가져와서 더함
        Vector3 currentMoveDirection = (movement / 5);

        // 적의 반대 방향과 현재 이동 방향을 합산한 방향으로 밀려남
        Vector3 combinedKnockbackDirection = (knockbackDirection + Vector3.up).normalized;

        // 해당 방향으로 힘을 가해 밀어냄
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
        // 흡수할 수 있는 오브젝트와 충돌했는지 확인
        if (collision.gameObject.GetComponentInParent<EatAbleObjectBase>())
        {
            CompareSize(collision.gameObject);

         
        }

   
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        float adjustedGroundCheckDistance = groundCheckDistance * transform.localScale.y;

        Gizmos.DrawRay(transform.position, Vector3.down * adjustedGroundCheckDistance);
        Gizmos.DrawRay(transform.position + (Vector3.right / 2.2f), Vector3.down * (adjustedGroundCheckDistance));
        Gizmos.DrawRay(transform.position - (Vector3.right / 2.5f), Vector3.down * adjustedGroundCheckDistance);
        Gizmos.DrawRay(transform.position + (Vector3.forward / 2.5f), Vector3.down * adjustedGroundCheckDistance);
        Gizmos.DrawRay(transform.position - (Vector3.forward / 2.5f), Vector3.down * adjustedGroundCheckDistance);

    }

    private void OnTriggerEnter(Collider collider)
    {
        // 흡수할 수 있는 오브젝트와 충돌했는지 확인
        if (collider.GetComponentInParent<EatAbleObjectBase>())
        {
            CompareSize(collider.gameObject);
        }

        if (collider.gameObject.CompareTag("Sea"))
        {
            AchievementManager.Instance.UpdateAchievement
               (AchievementManager.Instance.achievements[8].achievementName, 1);

            GameManager.Instance.GameOver();

            Destroy(gameObject);
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
