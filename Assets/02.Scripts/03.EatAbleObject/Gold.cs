using UnityEngine;
using System;

public class Gold : EatAbleObjectBase
{
    public Action<GameObject> OnDisableGold; // ��Ȱ��ȭ�� �� ȣ���� �̺�Ʈ

    private Vector3 DefaultSize;

    private void Awake()
    {
        DefaultSize = transform.localScale; // �ʱ� ������ �ʱ�ȭ
    }

    public override void Eaten(Transform slimeTrans)
    {
        base.Eaten(slimeTrans);

        GameManager.Instance.IncreaseMoney();
    }

    public override void Digested()
    {
        transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

        if (transform.localScale.x < 0.1f)
        {
            isGetEaten = !isGetEaten;
            transform.SetParent(null); // �θ� ����  
            gameObject.SetActive(false);
        }
    }

    private void OnEnable() // Ȱ��ȭ �� ������ ����
    {
        transform.localScale = DefaultSize;
        GetComponent<Collider>().enabled = true;

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
            OnDisableGold = null; // �̺�Ʈ �ʱ�ȭ�� �޸� ���� ����
        }
    }
}
