using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [LabelText("Ÿ��Ʋ UI"), SerializeField]
    private GameObject titleUI;

    void Start()
    {
        InputManager.Instance.SwitchToActionMap("UI"); // InputManager �׼� �� Title�� ��ü
    }

    void Update()
    {
        InputKey();
    }

    private void InputKey()
    {
        if ( titleUI.activeSelf && InputManager.Instance.anyKeyInput)
        {
            SceneManager.LoadScene(2); // ���� ������ �̵�
        }
    }
}
