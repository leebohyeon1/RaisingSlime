using Pathfinding;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EatAbleObjectBase : MonoBehaviour, IUpdateable
{
    [LabelText("사이즈"), SerializeField]
    private float size;

    [BoxGroup("먹혔을 때"), LabelText("슬라임 사이즈 증가량"), SerializeField] 
    private float slimeIncreaseSize;

    [BoxGroup("먹혔을 때"), LabelText("점수 증가량"), SerializeField]
    private float plusScore;

    [BoxGroup("먹혔을 때"), LabelText("초당 줄어드는 속도"), SerializeField] 
    protected float shrinkSpeed = 0.5f; // 크기가 줄어드는 속도 (수치를 조정해 천천히 감소하도록)

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

    public virtual void Eaten(Transform slimeTrans) // 먹히는 함수
    {
        if (isLock)
        {
            return;
        }

        isLock = true;
        isGetEaten = !isGetEaten;
      
        // 부모가 바뀌기 전의 월드 스케일을 저장
        Vector3 originalWorldScale = transform.lossyScale;

        // 부모를 변경
        transform.SetParent(slimeTrans);

        // 부모 변경 후 월드 스케일을 다시 원래대로 설정
        SetWorldScale(originalWorldScale);

        Vector3 randomPosition = new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(-0.3f, 0.3f),
            Random.Range(-0.3f, 0.3f)
        );

        // 로컬 포지션을 0으로 설정 (슬라임 중심에 배치)
        transform.localPosition = Vector3.zero + randomPosition;

        Collider[] colliders = GetComponentsInChildren<Collider>(); // 충돌 비활성화
        
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
            Destroy(GetComponent<Rigidbody>()); // Rigidbody가 있으면 비활성화
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

    // 월드 스케일을 유지하기 위한 함수
    private void SetWorldScale(Vector3 targetWorldScale)
    {
        // 부모의 스케일에 영향을 받지 않도록 로컬 스케일을 설정
        transform.localScale = Vector3.one; // 로컬 스케일을 임시로 1로 설정

        // 부모의 월드 스케일에 맞게 로컬 스케일을 다시 설정
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
