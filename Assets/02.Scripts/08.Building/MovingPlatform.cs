using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private Vector3 previousPosition; // ���� �������� �÷��� ��ġ
    private Vector3 platformVelocity;
    private bool isOnPlatform = false;
    private GameObject player;

    void Start()
    {
        // �÷��̾ ã�� ����
        //player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        // �÷����� ���� �ӵ� ��� (���� ��ġ - ���� ��ġ) / �ð�
        platformVelocity = (transform.position - previousPosition) / Time.deltaTime;
        // ���� �������� ���� ���� ��ġ�� ���� ��ġ�� ����
        previousPosition = transform.position;
        
        if (isOnPlatform)
        {
            // �÷��̾��� Rigidbody�� �÷����� �ӵ��� ������
            player.GetComponent<Rigidbody>().velocity += (platformVelocity.normalized / 5.7f);
        }

    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = collision.gameObject;
            // �÷��̾ �÷��� ���� ������ ǥ��
            isOnPlatform = true;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            //player = collision.gameObject;
            // �÷��̾ �÷������� ������
            isOnPlatform = false;
        }
    }
}
