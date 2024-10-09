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
    private float shrinkSpeed = 0.5f; // 크기가 줄어드는 속도 (수치를 조정해 천천히 감소하도록)

    private bool isGetEaten = false;

    protected virtual void Start()
    {
        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    public virtual void OnUpdate(float dt)
    {
        if (isGetEaten)
        {
            Digested();
        }
    }
 
    public void Eaten(Transform slimeTrans) // 먹히는 함수
    {
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

        GetComponentInChildren<Collider>().enabled = false; // 충돌 비활성화
        
        if(GetComponent<NavMeshAgent>())
        {
            GetComponent<NavMeshAgent>().enabled = false;
        }
        
        if (GetComponent<Rigidbody>())
        {
            Destroy(GetComponent<Rigidbody>()); // Rigidbody가 있으면 비활성화
        }
    }

    public bool GetEaten()
    {
        return isGetEaten;
    }

    public void Digested()
    {

        transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

        if (transform.localScale.magnitude < 0.1f)
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

    void OnDestroy()
    {
        GameLogicManager.Instance.DeregisterUpdatableObject(this);
    }
}
