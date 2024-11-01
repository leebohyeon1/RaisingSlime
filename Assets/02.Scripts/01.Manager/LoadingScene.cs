using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    public Slider progressBar;
    public static string sceneToLoad = "NextScene";  // ������ �ε��� �� �̸��� �����ϼ���.


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
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;  // �ε� �Ϸ� �� �ڵ� ��ȯ ����

        while (!operation.isDone)
        {
            // �ε� ���� ��Ȳ ǥ�� (0 ~ 1)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;

            if (operation.progress >= 0.9f)
            {
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
