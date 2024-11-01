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
        foreach (var updateable in _updateableObjects)
        {
            updateable.OnUpdate(dt);  // ��ϵ� ������Ʈ ������Ʈ ȣ��
        }
        
    }

    // ����Ʈ�� ������Ʈ �߰�
    public void RegisterUpdatableObject(IUpdateable obj)
    {
        if (obj != null && !_updateableObjects.Contains(obj))
        {
            _updateableObjects.Add(obj);
        }
    }

    // ����Ʈ�� ������Ʈ ����
    public void DeregisterUpdatableObject(IUpdateable obj)
    {
        if (_updateableObjects.Contains(obj))
        {
            _updateableObjects.Remove(obj);
        }
    }

    protected override void OnApplicationQuit()
    {


        // ����Ʈ ����
        _updateableObjects.Clear();

        System.GC.Collect();
        base.OnApplicationQuit();
    }
    protected override void OnDestroy()
    {   
        base.OnDestroy();

        _updateableObjects.Clear();
    }
}
