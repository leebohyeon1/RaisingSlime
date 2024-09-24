using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [LabelText("Ÿ��Ʋ UI"), SerializeField]
    private GameObject titleUI;
    [LabelText("���� UI"), SerializeField]
    private GameObject mainUI;
    [LabelText("�ɼ� UI"), SerializeField]
    private GameObject optionUI;

    void Start()
    {
        InputManager.Instance.SwitchToActionMap("Title"); // InputManager �׼� �� Title�� ��ü

        if (optionUI == null)
        {
            optionUI = OptionManager.Instance.OptionUI;
        }

        titleUI.SetActive(true);
        mainUI.SetActive(false);
        optionUI.SetActive(false);
    }

    void Update()
    {
        InputKey();
    }

    private void InputKey()
    {
        if ( titleUI.activeSelf && InputManager.Instance.anyKeyInput)
        {
            titleUI.SetActive(false);
            mainUI.SetActive(true);
        }
    }

    public void StartButton()  // ���� ���� ��ư
    {
        SceneManager.LoadScene(1);
    }

    public void ExitButton()   // ���� ���� ��ư
    {
        Application.Quit();
    }

    public void OptionButton() // ���� ��ư
    {
        if (!optionUI.activeSelf)
        {
            optionUI.SetActive(true);
        }
    }
}
