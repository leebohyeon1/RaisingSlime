using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Gold : EatAbleObjectBase
{
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
        gameObject.AddComponent<Rigidbody>();
    }
}
