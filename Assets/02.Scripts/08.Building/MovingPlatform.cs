using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private Vector3 previousPosition; // 이전 프레임의 플랫폼 위치
    private Vector3 platformVelocity;
    private bool isOnPlatform = false;
    private GameObject player;

    void Start()
    {
        // 플레이어를 찾아 저장
        //player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        // 플랫폼의 현재 속도 계산 (현재 위치 - 이전 위치) / 시간
        platformVelocity = (transform.position - previousPosition) / Time.deltaTime;
        // 다음 프레임을 위해 현재 위치를 이전 위치로 저장
        previousPosition = transform.position;
        
        if (isOnPlatform)
        {
            // 플레이어의 Rigidbody에 플랫폼의 속도를 더해줌
            player.GetComponent<Rigidbody>().velocity += (platformVelocity.normalized / 5.7f);
        }

    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = collision.gameObject;
            // 플레이어가 플랫폼 위에 있음을 표시
            isOnPlatform = true;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            //player = collision.gameObject;
            // 플레이어가 플랫폼에서 내려감
            isOnPlatform = false;
        }
    }
}
