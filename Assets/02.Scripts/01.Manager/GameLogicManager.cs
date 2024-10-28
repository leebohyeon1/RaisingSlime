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
        // 리스트의 모든 업데이트 실행
        float dt = Time.deltaTime;
        for (int i = 0; i < Instance._updateableObjects.Count; i++) 
        {
            Instance._updateableObjects[i].OnUpdate(dt);
        }
    }

    // 리스트에 오브젝트 추가
    public void RegisterUpdatableObject(IUpdateable obj)
    {
        if (!Instance._updateableObjects.Contains(obj))
        {
            Instance._updateableObjects.Add(obj);
        }
    }

    // 리스트에 오브젝트 삭제
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
