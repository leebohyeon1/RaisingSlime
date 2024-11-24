using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private Vector3 previousPosition; // 이전 프레임의 플랫폼 위치
    private Vector3 platformVelocity;
    private GameObject player;
    private bool isMovingPlayerToPlatform = false;

    [SerializeField] private Transform[] doors;

    // 문 이동을 위한 변수들
    private bool isMovingDoors = false;
    private Vector3 door0TargetPosition;
    private Vector3 door1TargetPosition;

    // 플레이어를 이동시킬 타겟 위치
    private Vector3 playerTargetPosition;

    void Start()
    {
        // 현재 위치로 previousPosition 초기화
        previousPosition = transform.position;
    }

    void Update()
    {
        // 플랫폼의 현재 속도 계산
        platformVelocity = (transform.position - previousPosition) / Time.deltaTime;
        // 다음 프레임을 위해 현재 위치 저장
        previousPosition = transform.position;

        // 필요한 경우 플레이어를 플랫폼으로 부드럽게 이동
        if (isMovingPlayerToPlatform && player != null)
        {
            // 플레이어의 현재 위치 가져오기
            Vector3 playerPosition = player.transform.position;
            // 플레이어를 타겟 위치로 이동
            float speed = 1f; // 필요에 따라 조정
            player.transform.position = Vector3.MoveTowards(playerPosition, playerTargetPosition, speed * Time.deltaTime);

            // 플레이어가 타겟 위치에 도달했는지 확인
            if (Vector3.Distance(player.transform.position, playerTargetPosition) < 0.01f)
            {
                // 이동 중지
                isMovingPlayerToPlatform = false;
         
                player.transform.position = playerTargetPosition;
                
            }
        }

        // 필요한 경우 문을 부드럽게 이동
        if (isMovingDoors)
        {
            float doorSpeed = 0.5f; // 필요에 따라 조정

            // door[0]을 타겟 위치로 이동
            doors[0].localPosition = Vector3.MoveTowards(doors[0].localPosition, door0TargetPosition, doorSpeed * Time.deltaTime);

            // door[1]을 타겟 위치로 이동
            doors[1].localPosition = Vector3.MoveTowards(doors[1].localPosition, door1TargetPosition, doorSpeed * Time.deltaTime);

            // 두 문이 타겟 위치에 도달했는지 확인
            bool door0Reached = Vector3.Distance(doors[0].localPosition, door0TargetPosition) < 0.01f;
            bool door1Reached = Vector3.Distance(doors[1].localPosition, door1TargetPosition) < 0.01f;

            if (door0Reached && door1Reached)
            {
                // 문 이동 중지
                isMovingDoors = false;
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            GetComponent<TransparentObject>().ResetOriginalTransparent();
            Destroy(GetComponent<TransparentObject>());

            GameManager.Instance.GameOverForAchievement();

            AchievementManager.Instance.UpdateAchievement
                (AchievementManager.Instance.achievements[0].achievementName, 1);

            player = collision.gameObject;
            player.transform.parent = transform;
            player.GetComponent<Rigidbody>().velocity = Vector3.zero;

            // 플레이어를 이동시킬 타겟 위치 설정 (충돌 시 플랫폼의 위치를 기준으로)
            Vector3 playerPosition = player.transform.position;
            playerTargetPosition = new Vector3(transform.position.x, playerPosition.y, transform.position.z);

            // 플레이어를 플랫폼으로 이동 시작
            isMovingPlayerToPlatform = true;

            // 문 이동 시작
            isMovingDoors = true;

            // 문들의 타겟 위치 설정 (현재 위치에서 상대적으로 이동)
            door0TargetPosition = doors[0].localPosition + new Vector3(0.5f, 0f, 0f); // x를 0.5 감소
            door1TargetPosition = doors[1].localPosition + new Vector3(-0.5f, 0f, 0f);  // x를 0.5 증가

            StartCoroutine(GameOver());
        }
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(10f);
        GameManager.Instance.GameOver();
    }
}
