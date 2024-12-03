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
    public static string sceneToLoad = "NextScene";  // 다음에 로드할 씬 이름을 설정하세요.
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
        operation.allowSceneActivation = false;  // 로딩 완료 후 자동 전환 방지

        tutorialImage[tutorialNum % 2].SetActive(true);
        tutorialImage[(tutorialNum + 1) % 2].SetActive(false);

        tutorialNum++;


        while (!operation.isDone)
        {
            // 로딩 진행 상황 표시 (0 ~ 1)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;

            if (operation.progress >= 0.9f)
            {
                text.gameObject.SetActive(true);
                // 아무 키나 누르면 씬 전환
                if (Input.anyKeyDown)
                {
                    operation.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }
}
