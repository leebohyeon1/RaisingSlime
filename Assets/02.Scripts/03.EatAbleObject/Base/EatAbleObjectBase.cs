using Pathfinding;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EatAbleObjectBase : MonoBehaviour, IUpdateable
{
    [LabelText("������"), SerializeField]
    private float size;

    [BoxGroup("������ ��"), LabelText("������ ������ ������"), SerializeField] 
    private float slimeIncreaseSize;

    [BoxGroup("������ ��"), LabelText("���� ������"), SerializeField]
    private float plusScore;

    [BoxGroup("������ ��"), LabelText("�ʴ� �پ��� �ӵ�"), SerializeField] 
    protected float shrinkSpeed = 0.5f; // ũ�Ⱑ �پ��� �ӵ� (��ġ�� ������ õõ�� �����ϵ���)

    protected bool isLock = false;
    protected bool isGetEaten = false;
    protected bool isTrigger = false;

    protected float timer;
    protected float checkTime = 0.5f;

    public virtual void Start()
    {
        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    public virtual void OnUpdate(float dt)
    {
        if (isGetEaten)
        {
            Digested();
        }
     
        if (transform.position.y < -5f && !isGetEaten)
        {
            Eaten(transform);
        }
    }

    public virtual void Eaten(Transform slimeTrans) // ������ �Լ�
    {
        if (isLock)
        {
            return;
        }

        isLock = true;
        isGetEaten = !isGetEaten;

     

        // �θ� �ٲ�� ���� ���� �������� ����
        Vector3 originalWorldScale = transform.lossyScale;

        // �θ� ����
        transform.SetParent(slimeTrans);

        // �θ� ���� �� ���� �������� �ٽ� ������� ����
        SetWorldScale(originalWorldScale);

        Vector3 randomPosition = new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(-0.3f, 0.3f),
            Random.Range(-0.3f, 0.3f)
        );

        // ���� �������� 0���� ���� (������ �߽ɿ� ��ġ)
        transform.localPosition = Vector3.zero + randomPosition;

        Collider[] colliders = GetComponentsInChildren<Collider>(); // �浹 ��Ȱ��ȭ
        
        foreach(Collider collider in colliders)
        {
            collider.enabled = false;
        }

        if(GetComponent<AIPath>())
        {
            GetComponent<AIPath>().enabled = false;

            AchievementManager.Instance.UpdateAchievement
             (AchievementManager.Instance.achievements[0].achievementName, 1);
        }
        
        if(GetComponentInChildren<NavmeshCut>())
        {
            NavmeshCut[] obstacles = GetComponentsInChildren<NavmeshCut>();

            foreach (NavmeshCut obstacle in obstacles)
            {
                obstacle.enabled = false;
            }
        }

        if (GetComponent<Rigidbody>())
        {
            Destroy(GetComponent<Rigidbody>()); // Rigidbody�� ������ ��Ȱ��ȭ
        }

        if (GetComponent<Citizen>() || GetComponent<TutorialCitizen>())
        {
            AchievementManager.Instance.UpdateAchievement
                (AchievementManager.Instance.achievements[4].achievementName, 1);
        }
    }

    public virtual bool GetEaten()
    {
        return isGetEaten;
    }

    public virtual void Digested()
    {

        transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

        if (transform.localScale.x < 0.1f)
        {
            Destroy(gameObject);
        }
    }

    // ���� �������� �����ϱ� ���� �Լ�
    private void SetWorldScale(Vector3 targetWorldScale)
    {
        // �θ��� �����Ͽ� ������ ���� �ʵ��� ���� �������� ����
        transform.localScale = Vector3.one; // ���� �������� �ӽ÷� 1�� ����

        // �θ��� ���� �����Ͽ� �°� ���� �������� �ٽ� ����
        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(
            targetWorldScale.x / parentScale.x,
            targetWorldScale.y / parentScale.y,
            targetWorldScale.z / parentScale.z
        );
    }
   
    public float GetSize()
    {
        return size;
    }

    public Vector3 SlimeIncreaseSize()
    {
        return new Vector3(slimeIncreaseSize, slimeIncreaseSize, slimeIncreaseSize);
    }

    public float GetPlusScore()
    {
        return plusScore;
    }

    protected virtual void OnDestroy()
    {
        if (GameLogicManager.Instance != null)
        {
            GameLogicManager.Instance.DeregisterUpdatableObject(this);
        }
    }

    private void OnApplicationQuit()
    {
        if (GameLogicManager.Instance != null)
        {
            GameLogicManager.Instance.DeregisterUpdatableObject(this);
        }
    }
}
