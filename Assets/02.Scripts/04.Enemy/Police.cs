using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Police : EnemyBase
{


    [BoxGroup("����"), LabelText("���� �ӵ�"), SerializeField, Range(1f, 20f)]
    private float attackSpeed = 2f;

    [BoxGroup("����"), LabelText("���� ����"), SerializeField, Range(1f, 20f)]
    private float attackRange = 10f;

    [BoxGroup("����"), LabelText("�Ѿ� ������"), SerializeField]
    private GameObject bulletPrefab;

    [BoxGroup("����"), LabelText("�Ѿ� �ӵ�"), SerializeField, Range(1f, 20f)]
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
