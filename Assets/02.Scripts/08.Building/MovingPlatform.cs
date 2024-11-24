using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private Vector3 previousPosition; // ���� �������� �÷��� ��ġ
    private Vector3 platformVelocity;
    private GameObject player;
    private bool isMovingPlayerToPlatform = false;

    [SerializeField] private Transform[] doors;

    // �� �̵��� ���� ������
    private bool isMovingDoors = false;
    private Vector3 door0TargetPosition;
    private Vector3 door1TargetPosition;

    // �÷��̾ �̵���ų Ÿ�� ��ġ
    private Vector3 playerTargetPosition;

    void Start()
    {
        // ���� ��ġ�� previousPosition �ʱ�ȭ
        previousPosition = transform.position;
    }

    void Update()
    {
        // �÷����� ���� �ӵ� ���
        platformVelocity = (transform.position - previousPosition) / Time.deltaTime;
        // ���� �������� ���� ���� ��ġ ����
        previousPosition = transform.position;

        // �ʿ��� ��� �÷��̾ �÷������� �ε巴�� �̵�
        if (isMovingPlayerToPlatform && player != null)
        {
            // �÷��̾��� ���� ��ġ ��������
            Vector3 playerPosition = player.transform.position;
            // �÷��̾ Ÿ�� ��ġ�� �̵�
            float speed = 1f; // �ʿ信 ���� ����
            player.transform.position = Vector3.MoveTowards(playerPosition, playerTargetPosition, speed * Time.deltaTime);

            // �÷��̾ Ÿ�� ��ġ�� �����ߴ��� Ȯ��
            if (Vector3.Distance(player.transform.position, playerTargetPosition) < 0.01f)
            {
                // �̵� ����
                isMovingPlayerToPlatform = false;
         
                player.transform.position = playerTargetPosition;
                
            }
        }

        // �ʿ��� ��� ���� �ε巴�� �̵�
        if (isMovingDoors)
        {
            float doorSpeed = 0.5f; // �ʿ信 ���� ����

            // door[0]�� Ÿ�� ��ġ�� �̵�
            doors[0].localPosition = Vector3.MoveTowards(doors[0].localPosition, door0TargetPosition, doorSpeed * Time.deltaTime);

            // door[1]�� Ÿ�� ��ġ�� �̵�
            doors[1].localPosition = Vector3.MoveTowards(doors[1].localPosition, door1TargetPosition, doorSpeed * Time.deltaTime);

            // �� ���� Ÿ�� ��ġ�� �����ߴ��� Ȯ��
            bool door0Reached = Vector3.Distance(doors[0].localPosition, door0TargetPosition) < 0.01f;
            bool door1Reached = Vector3.Distance(doors[1].localPosition, door1TargetPosition) < 0.01f;

            if (door0Reached && door1Reached)
            {
                // �� �̵� ����
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

            // �÷��̾ �̵���ų Ÿ�� ��ġ ���� (�浹 �� �÷����� ��ġ�� ��������)
            Vector3 playerPosition = player.transform.position;
            playerTargetPosition = new Vector3(transform.position.x, playerPosition.y, transform.position.z);

            // �÷��̾ �÷������� �̵� ����
            isMovingPlayerToPlatform = true;

            // �� �̵� ����
            isMovingDoors = true;

            // ������ Ÿ�� ��ġ ���� (���� ��ġ���� ��������� �̵�)
            door0TargetPosition = doors[0].localPosition + new Vector3(0.5f, 0f, 0f); // x�� 0.5 ����
            door1TargetPosition = doors[1].localPosition + new Vector3(-0.5f, 0f, 0f);  // x�� 0.5 ����

            StartCoroutine(GameOver());
        }
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(10f);
        GameManager.Instance.GameOver();
    }
}
