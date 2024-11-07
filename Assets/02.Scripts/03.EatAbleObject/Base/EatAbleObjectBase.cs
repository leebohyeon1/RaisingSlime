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

    private PlayerStat playerStat;

    protected virtual void Start()
    {
        GameLogicManager.Instance.RegisterUpdatableObject(this);

       Invoke("SetPlayer", 0.1f);
    }

    public virtual void OnUpdate(float dt)
    {
        if (isGetEaten)
        {
            Digested();
        }
        else
        {
            //CheckSize();
        }
       
    }
 
    protected virtual void SetPlayer()
    {
        playerStat = SpawnManager.Instance.GetPlayerTrans().GetComponent<PlayerStat>();
    }

    protected virtual void CheckSize()
    {
        if (playerStat == null)
        {
            return;
        }

        timer += Time.deltaTime;

        if (checkTime < timer)
        {
            timer = 0f;
            
            if (!isTrigger && size <= playerStat.curSize)
            {
                isTrigger = true;
                Collider[] colliders = GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    collider.isTrigger = isTrigger;
                }
            }
            else if (isTrigger && size > playerStat.curSize)
            {
                isTrigger = false;
                Collider[] colliders = GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    collider.isTrigger = isTrigger;
                }
            }

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
}
