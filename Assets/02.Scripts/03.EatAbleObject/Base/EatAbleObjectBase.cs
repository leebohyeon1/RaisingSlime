using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatAbleObjectBase : MonoBehaviour
{
    public float size;

    [SerializeField] private float slimeIncreaseSize;

    public float slimeRecoveryAmount;

    private bool isGetEaten = false;

    // 추가: 크기 감소 속도를 조정하는 변수
    [SerializeField] private float shrinkSpeed = 0.5f; // 크기가 줄어드는 속도 (수치를 조정해 천천히 감소하도록)

    protected virtual void Update()
    {
        if(isGetEaten)
        {

            Digested();
        }

    }

    public Vector3 SlimeIncreaseSize()
    {
        return new Vector3(slimeIncreaseSize, slimeIncreaseSize, slimeIncreaseSize);
    }

    public void GetEaten()
    {
        isGetEaten = !isGetEaten;
    }

    public void Digested()
    {
        transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

        if (transform.localScale.magnitude < 0.1f)
        {
            Destroy(gameObject);
        }
    }


}
