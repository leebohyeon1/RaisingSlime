using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    [LabelText("���� UI"), SerializeField]
    private GameObject mainUI;
    [LabelText("�ɼ� UI"), SerializeField]
    private GameObject optionUI;

    void Start()
    {
        if (optionUI == null)
        {
            optionUI = OptionManager.Instance.OptionUI;
        }

        mainUI.SetActive(true);
        optionUI.SetActive(false);
    }

    void Update()
    {
        
    }

    public void StartButton()  // ���� ���� ��ư
    {
        SceneManager.LoadScene(2);
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
