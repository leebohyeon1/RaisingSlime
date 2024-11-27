using Pathfinding.RVO;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFO : NPCBase
{
    [BoxGroup("UFO"), LabelText("�ʴ� ���ݷ�"), SerializeField, Range(0.1f, 100f)]
    private float damagePerSecond = 0.2f; 

    [BoxGroup("UFO"), LabelText("ȸ�� �ӵ�"), SerializeField]
    private float rotationSpeed = 50f; // ȸ�� �ӵ�
    
    [BoxGroup("UFO"), LabelText("���� �Ÿ�"), SerializeField]
    private float radius = 5f; // UFO�� �÷��̾�κ��� ������ �Ÿ�(�ݰ�)
    
    [BoxGroup("UFO"), LabelText("����"), SerializeField]
    private float heightFactor = 2f; // �÷��̾� ũ�⿡ ����� ���� ����

    [BoxGroup("UFO"), LabelText("�߻� ��ġ"), SerializeField]
    private Transform firePos;

    [BoxGroup("UFO"), LabelText("��"), SerializeField]
    private GameObject lineRendererPrefab;
    [BoxGroup("UFO"), LabelText("�� ��"), SerializeField]
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

    // UFO ����
    private void MoveUFO()
    {
        // �÷��̾��� ũ�⿡ ����Ͽ� ���̸� ����
        float adjustedHeight = target.localScale.y * heightFactor;

        // UFO�� �÷��̾� ������ ȸ����ŵ�ϴ�.
        transform.RotateAround(target.position, Vector3.up, rotationSpeed * Time.deltaTime);

        // �ݰ��� �����ϸ� ���̵� �÷��̾� ũ�⿡ ����Ͽ� ����
        Vector3 newPosition = transform.position - target.position;
        newPosition = newPosition.normalized * radius; // �Ÿ� ����
        newPosition.y = adjustedHeight; // ���� ����

        transform.position = target.position + newPosition; // �÷��̾� �������� ����
    }

    // ���������� �÷��̾� ����
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
