using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCParent : MonoBehaviour
{
    public delegate void DestroyedHandler();
    public event DestroyedHandler OnDestroyed;

    public GameObject npc;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckNPC();
    }

    void CheckNPC()
    {
        if(npc == null)
        {
            Destroy(gameObject);
        }
    }

    public void SetNPCTarget(Transform transform) //플레이어 타겟 직접 설정
    {
        npc.GetComponent<NPCBase>().SetTarget(transform);
    }


    private void OnDestroy()
    {
        if (OnDestroyed != null)
        {
            OnDestroyed.Invoke(); // 적이 파괴될 때 이벤트 발생
        }
    }
}
