using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Gold : EatAbleObjectBase
{
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

        if (transform.localScale.x < 0.1f)
        {
            isGetEaten = !isGetEaten;
            transform.SetParent(null); // 부모를 없앰
            gameObject.SetActive(false);
        }
    }

    private void OnEnable() // 활성화 시 스케일 변경
    {
        transform.localScale = DefaultSize;
        GetComponent<Collider>().enabled = true;
        gameObject.AddComponent<Rigidbody>();
    }
}
