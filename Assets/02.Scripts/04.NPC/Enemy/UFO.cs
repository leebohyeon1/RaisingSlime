using INab.Dissolve;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFO : NPCBase
{
    [BoxGroup("UFO"), LabelText("초당 공격력"), SerializeField, Range(0.1f, 100f)]
    private float damagePerSecond = 0.2f; 

    [BoxGroup("UFO"), LabelText("회전 속도"), SerializeField]
    private float rotationSpeed = 50f; // 회전 속도
    
    [BoxGroup("UFO"), LabelText("유지 거리"), SerializeField]
    private float radius = 5f; // UFO가 플레이어로부터 유지할 거리(반경)
    
    [BoxGroup("UFO"), LabelText("높이"), SerializeField]
    private float heightFactor = 2f; // 플레이어 크기에 비례한 높이 조정

    private Player player;

    protected override void Awake()
    {

    }

    protected override void Start()
    {
        base.Start();

        player = target.GetComponent<Player>();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void enemyAction()
    {
        if (target ==  null)
        {
            return;
        }

        MoveUFO();

        Attack();
    }

    // UFO 공전
    private void MoveUFO()
    {
        // 플레이어의 크기에 비례하여 높이를 설정
        float adjustedHeight = target.localScale.y * heightFactor;

        // UFO를 플레이어 주위로 회전시킵니다.
        transform.RotateAround(target.position, Vector3.up, rotationSpeed * Time.deltaTime);

        // 반경을 유지하며 높이도 플레이어 크기에 비례하여 유지
        Vector3 newPosition = transform.position - target.position;
        newPosition = newPosition.normalized * radius; // 거리 유지
        newPosition.y = adjustedHeight; // 높이 유지

        transform.position = target.position + newPosition; // 플레이어 주위에서 공전
    }

    // 지속적으로 플레이어 공격
    private void Attack()
    {
        float damage = damagePerSecond * Time.deltaTime;

        player.TakeDamage(damage);
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }
    private void OnTriggerExit(Collider other)
    {

    }
}
