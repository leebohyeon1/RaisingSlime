using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [LabelText("타이틀 UI"), SerializeField]
    private GameObject titleUI;

    void Start()
    {
        InputManager.Instance.SwitchToActionMap("UI"); // InputManager 액션 맵 Title로 교체
    }

    void Update()
    {
        InputKey();
    }

    private void InputKey()
    {
        if ( titleUI.activeSelf && InputManager.Instance.anyKeyInput)
        {
            SceneManager.LoadScene(2); // 메인 씬으로 이동
        }
    }
}
