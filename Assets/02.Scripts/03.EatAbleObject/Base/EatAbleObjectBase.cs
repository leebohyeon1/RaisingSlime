using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EatAbleObjectBase : MonoBehaviour
{
    [LabelText("������"), SerializeField]
    private float size;

    [BoxGroup("������ ��"), LabelText("������ ������ ������"), SerializeField] 
    private float slimeIncreaseSize;

    [BoxGroup("������ ��"), LabelText("���� ������"), SerializeField]
    private float plusScore;

    [BoxGroup("������ ��"), LabelText("�ʴ� �پ��� �ӵ�"), SerializeField] 
    private float shrinkSpeed = 0.5f; // ũ�Ⱑ �پ��� �ӵ� (��ġ�� ������ õõ�� �����ϵ���)

    private bool isGetEaten = false;

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
        if(isGetEaten)
        {
            Digested();
        }

    }

    public void Eaten(Transform slimeTrans) // ������ �Լ�
    {
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

        GetComponentInChildren<Collider>().enabled = false; // �浹 ��Ȱ��ȭ
        GetComponent<NavMeshAgent>().enabled = false;
        if (GetComponent<Rigidbody>() != null)
        {
            Destroy(GetComponent<Rigidbody>()); // Rigidbody�� ������ ��Ȱ��ȭ
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
}
