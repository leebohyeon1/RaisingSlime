using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [LabelText("Ÿ��Ʋ UI"), SerializeField]
    private GameObject TitleUI;
    [LabelText("���� UI"), SerializeField]
    private GameObject MainUI;

    void Start()
    {
        InputManager.Instance.SwitchToActionMap("Title"); // InputManager �׼� �� Title�� ��ü
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
