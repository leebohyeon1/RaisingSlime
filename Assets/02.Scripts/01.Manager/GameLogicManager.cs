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

        // 씬 로드 시 오브젝트 정리
        SceneManager.sceneUnloaded += OnSceneUnloaded;
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

    // 씬이 언로드될 때 호출되는 메서드
    private void OnSceneUnloaded(Scene scene)
    {
        // 모든 오브젝트 정리
        _updateableObjects.Clear();
    }

    protected override void OnApplicationQuit()
    {
        // 씬 이벤트 해제
        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        // 리스트 정리
        _updateableObjects.Clear();

        System.GC.Collect();
        base.OnApplicationQuit();
    }
    protected override void OnDestroy()
    {
        // 씬 이벤트 해제
        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        // 리스트 정리
        _updateableObjects.Clear();

        System.GC.Collect();
        
        base.OnDestroy();
    }
}
