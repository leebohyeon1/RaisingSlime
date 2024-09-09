using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatAbleObjectBase : MonoBehaviour
{
    public float size;

    [SerializeField] private float slimeIncreaseSize;

    public float slimeRecoveryAmount;

    private bool isGetEaten = false;

    // �߰�: ũ�� ���� �ӵ��� �����ϴ� ����
    [SerializeField] private float shrinkSpeed = 0.5f; // ũ�Ⱑ �پ��� �ӵ� (��ġ�� ������ õõ�� �����ϵ���)

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
