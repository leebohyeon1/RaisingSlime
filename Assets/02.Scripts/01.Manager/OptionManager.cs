using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionManager : MonoBehaviour
{
    public static OptionManager Instance { get; private set; } // 싱글턴 선언

    public GameObject OptionUI;


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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExitOption()
    {
        if (OptionUI.activeSelf)
        {
            Time.timeScale = 1.0f;  // 시간 재생
            OptionUI.SetActive(false); // 옵션 UI 끄기
        }
    }
}
