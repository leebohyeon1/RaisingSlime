using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLogicManager : Singleton<GameLogicManager>
{
    List<IUpdateable> _updateableObjects = new List<IUpdateable>();

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        // ����Ʈ�� ��� ������Ʈ ����
        float dt = Time.deltaTime;
        for (int i = 0; i < Instance._updateableObjects.Count; i++) 
        {
            Instance._updateableObjects[i].OnUpdate(dt);
        }
    }

    // ����Ʈ�� ������Ʈ �߰�
    public void RegisterUpdatableObject(IUpdateable obj)
    {
        if (!Instance._updateableObjects.Contains(obj))
        {
            Instance._updateableObjects.Add(obj);
        }
    }

    // ����Ʈ�� ������Ʈ ����
    public void DeregisterUpdatableObject(IUpdateable obj)
    {
        if (Instance._updateableObjects.Contains(obj))
        {
            Instance._updateableObjects.Remove(obj);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
