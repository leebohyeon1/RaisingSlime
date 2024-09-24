using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [LabelText("타이틀 UI"), SerializeField]
    private GameObject TitleUI;
    [LabelText("메인 UI"), SerializeField]
    private GameObject MainUI;

    void Start()
    {
        InputManager.Instance.SwitchToActionMap("Title"); // InputManager 액션 맵 Title로 교체
    }

    void Update()
    {
        InputKey();
    }

    private void InputKey()
    {
        if ( TitleUI.activeSelf && InputManager.Instance.anyKeyInput)
        {
            TitleUI.SetActive(false);
            MainUI.SetActive(true);
        }
    }
}
