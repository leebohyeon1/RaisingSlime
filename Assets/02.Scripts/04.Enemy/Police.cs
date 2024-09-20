using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Police : EnemyBase
{


    [BoxGroup("°æÂû"), LabelText("°ø°Ý ¼Óµµ"), SerializeField, Range(1f, 20f)]
    private float attackSpeed = 2f;

    [BoxGroup("°æÂû"), LabelText("°ø°Ý ¹üÀ§"), SerializeField, Range(1f, 20f)]
    private float attackRange = 10f;

    [BoxGroup("°æÂû"), LabelText("ÃÑ¾Ë ÇÁ¸®ÆÕ"), SerializeField]
    private GameObject bulletPrefab;

    [BoxGroup("°æÂû"), LabelText("ÃÑ¾Ë ¼Óµµ"), SerializeField, Range(1f, 20f)]
    private float bulletSpeed = 10f;


    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void enemyAction()
    {
        if (eatAbleObjectBase.GetEaten())
        {
            return;
        }

        agent.SetDestination(targetPos());

        float distanceToPlayer = Vector3.Distance(transform.position, targetPos());

        if (distanceToPlayer < attackRange)
        {
            agent.isStopped = true;

        }
        else if(distanceToPlayer >= attackRange) 
        {
            agent.isStopped = false;
        
        }
    }
}
