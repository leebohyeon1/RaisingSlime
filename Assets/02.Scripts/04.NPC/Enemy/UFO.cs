using Pathfinding.RVO;
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

    [BoxGroup("UFO"), LabelText("발사 위치"), SerializeField]
    private Transform firePos;

    [BoxGroup("UFO"), LabelText("빔"), SerializeField]
    private GameObject lineRendererPrefab;
    [BoxGroup("UFO"), LabelText("빔 끝"), SerializeField]
    private GameObject beamEndPrefab;


    [Header("Adjustable Variables")]
    public float beamEndOffset = 1f; //How far from the raycast hit point the end effect is positioned
    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
    public float textureLengthScale = 3; //Length of the beam texture

    private GameObject beamEnd;
    private GameObject beam;
    private LineRenderer line;

    private Player player;

    protected override void Awake()
    {
        eatAbleObjectBase = GetComponent<EatAbleObjectBase>();
    }

    protected override void Start()
    {
        if (target == null)
        {
            target = FindFirstObjectByType<Player>().transform;
        }


        player = target.GetComponent<Player>();

        beamEnd = Instantiate(beamEndPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        beam = Instantiate(lineRendererPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        line = beam.GetComponent<LineRenderer>();

        GameLogicManager.Instance.RegisterUpdatableObject(this);


        AchievementManager.Instance.UpdateAchievement
            (AchievementManager.Instance.achievements[3].achievementName, 1);

        AudioManager.Instance.PlaySFX("Laser",true);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        AudioManager.Instance.StopSFX("Laser");
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
        ShootBeamInDir(firePos.position, target.position - firePos.position);
        float damage = damagePerSecond * Time.deltaTime;

        player.TakeDamage(damage);
    }


    void ShootBeamInDir(Vector3 start, Vector3 dir)
    {
        line.positionCount = 2;
        line.SetPosition(0, start);

        Vector3 end = Vector3.zero;
        RaycastHit hit;
        if (Physics.Raycast(start, dir, out hit))
            end = hit.point - (dir.normalized * beamEndOffset);
        else
            end = transform.position + (dir * 100);

        beamEnd.transform.position = end;
        line.SetPosition(1, end);

        beamEnd.transform.LookAt(firePos.position);

        float distance = Vector3.Distance(start, end);
        line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
    }
}
