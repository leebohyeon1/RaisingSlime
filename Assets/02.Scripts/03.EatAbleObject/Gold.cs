using UnityEngine;
using System;

public class Gold : EatAbleObjectBase
{
    public Action<GameObject> OnDisableGold; // 비활성화될 때 호출할 이벤트

    private Vector3 DefaultSize;

    private void Awake()
    {
        DefaultSize = transform.localScale; // 초기 사이즈 초기화
    }

    public override void Eaten(Transform slimeTrans)
    {
        base.Eaten(slimeTrans);

        GameManager.Instance.IncreaseMoney();
    }

    public override void Digested()
    {
        transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

        if (transform.localScale.x < 0.2f)
        {
            isGetEaten = !isGetEaten;
            transform.SetParent(null); // 부모를 없앰  
            gameObject.SetActive(false);
        }
    }

    private void OnEnable() // 활성화 시 스케일 변경
    {
        transform.localScale = DefaultSize;
        Collider[] colliders = GetComponents<Collider>();

        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }

        if(!GetComponent<Rigidbody>())
        {
            gameObject.AddComponent<Rigidbody>();
        }
        
    }

  
    private void OnDisable()
    {
        if (OnDisableGold != null)
        {
            OnDisableGold.Invoke(gameObject);
            OnDisableGold = null; // 이벤트 초기화로 메모리 누수 방지
        }
    }
}
