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
        foreach (var updateable in _updateableObjects)
        {
            updateable.OnUpdate(dt);  // 등록된 오브젝트 업데이트 호출
        }
        
    }

    // 리스트에 오브젝트 추가
    public void RegisterUpdatableObject(IUpdateable obj)
    {
        if (obj != null && !_updateableObjects.Contains(obj))
        {
            _updateableObjects.Add(obj);
        }
    }

    // 리스트에 오브젝트 삭제
    public void DeregisterUpdatableObject(IUpdateable obj)
    {
        if (_updateableObjects.Contains(obj))
        {
            _updateableObjects.Remove(obj);
        }
    }

    protected override void OnApplicationQuit()
    {


        // 리스트 정리
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
