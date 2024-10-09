using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameLogicManager : MonoBehaviour
{
    public static GameLogicManager Instance { get; private set; }

    List<IUpdateable> _updateableObjects = new List<IUpdateable>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < Instance._updateableObjects.Count; i++) 
        {
            Instance._updateableObjects[i].OnUpdate(dt);
        }
    }

    public void RegisterUpdatableObject(IUpdateable obj)
    {
        if (!Instance._updateableObjects.Contains(obj))
        {
            Instance._updateableObjects.Add(obj);
        }
    }

    public void DeregisterUpdatableObject(IUpdateable obj)
    {
        if (Instance._updateableObjects.Contains(obj))
        {
            Instance._updateableObjects.Remove(obj);
        }
    }
}
