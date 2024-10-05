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
    
    [BoxGroup("땅 체크"),LabelText("땅에 있는가?"), SerializeField]
    private bool isGrounded = true; // 땅에 있는지 확인하는 플래그
    [BoxGroup("땅 체크"), LabelText("땅 체크 거리"), SerializeField]
    private float groundCheckDistance = 0.1f; // Raycast의 거리
    [BoxGroup("땅 체크"), LabelText("땅 레이어"), SerializeField]
    private LayerMask groundLayer; // 땅 체크를 위한 레이어 마스크
    [BoxGroup("땅 체크"), LabelText("땅 체크 위치"), SerializeField]
    private Transform groundCheckPosition; // Raycast 시작 위치 (플레이어 발밑)

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


        AutoDecreaseSize();
        HandleJump();

       // CheckNavMesh(30f);
    }

    private void FixedUpdate()
    {
        GroundCheck();

        HandleMovement();

        ApplyExtraGravity(); // 공중에 있을 때 중력 가속도 적용
    }

    // 땅에 닿았는지 확인하는 함수
    private void GroundCheck()
    {
        // 플레이어의 현재 크기에 따라 groundCheckDistance를 동적으로 조정
        float adjustedGroundCheckDistance = groundCheckDistance * transform.localScale.y;

        // 플레이어 중심에서 아래로 Ray를 쏴서 땅에 닿았는지 확인
        isGrounded = Physics.Raycast(transform.position, Vector3.down, adjustedGroundCheckDistance, groundLayer);

        if (!isGrounded && !playerStat.canJump)
        {
            playerStat.canJump = true; // 점프 초기화
        }

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

        // 이동 시 관성 고려하여 rb.velocity로 직접 이동 처리
        movement = playerMovement.Move(InputManager.Instance.moveInput, playerStat.moveSpeed);

        // 플레이어 이동 방향으로 회전
        /*if (movement != Vector3.zero) // 이동 중인 경우에만 회전
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement); // 이동 방향을 바라보도록 회전 목표 설정
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * playerStat.rotationSpeed); // 부드럽게 회전
        }*/

        rb.AddForce(movement * Time.deltaTime, ForceMode.VelocityChange);
    }

    private void HandleJump()
    {
        if (InputManager.Instance.jumpInput && isGrounded && playerStat.canJump) 
        {
            rb.AddForce(playerMovement.Jump(playerStat.jumpForce), ForceMode.Impulse);
            playerStat.canJump = false; // 점프 후 땅에 있지 않음
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
        else
        {
            if(eatAble.GetComponentInParent<NPCBase>().isEnemy) // 자신 보다 사이즈가 크고, 적일 경우
            {
                TakeDamage(eatAble.GetComponentInParent<NPCBase>().collisionDamage);
                KnockbackFromEnemy(eatAble); // 적 반대 방향으로 날아가기
            }
        }
    }

    private void Eat(EatAbleObjectBase eatAbleObjectBase) 
    {
        // 먹을 수 있는 오브젝트를 자식 오브젝트에 추가
        eatAbleObjectBase.Eaten(transform);

        // 슬라임의 크기를 증가
        transform.localScale += eatAbleObjectBase.SlimeIncreaseSize();

        // 점수 증가
        GameManager.Instance.IncreaseScore(eatAbleObjectBase.GetPlusScore());

        // 크기를 체크하는 변수 초기화
        playerStat.curSize = transform.localScale.x;
    }

    public void TakeDamage(float damage)
    {
        transform.localScale -= new Vector3(damage, damage, damage);
    }

    // 적과 충돌했을 때 밀려나는 기능
    private void KnockbackFromEnemy(GameObject enemy)
    {
        // 적의 위치에서 플레이어 위치로 향하는 반대 방향 벡터 계산
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 knockbackDirection = (transform.position - enemyPosition).normalized;

        // 현재 이동 방향을 가져와서 더함
        Vector3 currentMoveDirection = ( movement / 5) ;

        // 적의 반대 방향과 현재 이동 방향을 합산한 방향으로 밀려남
        Vector3 combinedKnockbackDirection = (knockbackDirection  + Vector3.up).normalized;

        // 해당 방향으로 힘을 가해 밀어냄
        rb.AddForce(combinedKnockbackDirection * playerStat.knockbackForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 흡수할 수 있는 오브젝트와 충돌했는지 확인
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
