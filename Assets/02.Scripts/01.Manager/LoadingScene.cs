using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

public class LoadingScene : MonoBehaviour
{
    [SerializeField]    
    private Slider progressBar;
    [SerializeField]
    private TMP_Text text;
    public static string sceneToLoad = "NextScene";  // ������ �ε��� �� �̸��� �����ϼ���.
    [SerializeField]
    private GameObject[] tutorialImage;
    public static int tutorialNum = 0; 


    private void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    public static void LoadScene(string sceneName)
    {
        sceneToLoad = sceneName;
        SceneManager.LoadScene("05.LoadingScene");
    }

    IEnumerator LoadSceneAsync()
    {
        text.gameObject.SetActive(false);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;  // �ε� �Ϸ� �� �ڵ� ��ȯ ����

        tutorialImage[tutorialNum % 2].SetActive(true);
        tutorialImage[(tutorialNum + 1) % 2].SetActive(false);

        tutorialNum++;


        while (!operation.isDone)
        {
            // �ε� ���� ��Ȳ ǥ�� (0 ~ 1)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;

            if (operation.progress >= 0.9f)
            {
                text.gameObject.SetActive(true);
                // �ƹ� Ű�� ������ �� ��ȯ
                if (Input.anyKeyDown)
                {
                    operation.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }
}
