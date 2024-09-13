using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    private NavMeshAgent agent;
    private EatAbleObjectBase eatAbleObjectBase;

    private Transform target;

    public delegate void DestroyedHandler();
    public event DestroyedHandler OnDestroyed;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        eatAbleObjectBase = GetComponent<EatAbleObjectBase>();

        target = FindFirstObjectByType<Player>().transform;
    }

    // Update is called once per frame
    void Update()
    {
       if(eatAbleObjectBase.GetEaten())
       {
            return;
       }

        agent.SetDestination(targetPos());
    }

    private Vector3 targetPos()
    {
        return new Vector3(target.position.x,0.0f,target.position.z);
    }


    private void OnDestroy()
    {
        if (OnDestroyed != null)
        {
            OnDestroyed.Invoke(); // 적이 파괴될 때 이벤트 발생
        }
    }
}
