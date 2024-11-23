using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DrawManager : MonoBehaviour
{
    [SerializeField]
    private uint drawPrice;
    [SerializeField]
    private TMP_Text moneyText;
    [SerializeField]
    private Button[] Buttons;
    
    private uint haveMoney;

    private GameObject newSkin;

    [SerializeField]private DrawMachine drawMachine;
    [SerializeField] private PlayableDirector playableDirector;
    private PlayableDirector capsuleDirector;

    [SerializeField] private GameObject prefab; // 생성할 오브젝트 프리팹
    [SerializeField] private Transform spawnPoint; // 생성 위치
    private GameObject spawnedObject;

    private void Start()
    {
        Init();

    }

    private void Update()
    {
        
    }

    private void OnDestroy()
    {
        if (playableDirector != null)
        {
            // 이벤트 등록 해제 (안전하게 처리)
            playableDirector.stopped -= OnTimelineStopped;
        }
    }

    private void Init()
    {
        GameData gameData = SaveManager.Instance.LoadPlayerData();
        haveMoney = gameData.money;
        moneyText.text = "Money: " + haveMoney;

        Buttons[0].onClick.AddListener(() => Draw(Buttons[0]));
        Buttons[1].onClick.AddListener(() => Exit());
    }

    private void Draw(Button button)
    {
        List<GameObject> skinList = SkinManager.Instance.GetCloseSlime();

        bool canDraw = haveMoney >= drawPrice && skinList.Count > 0;

        if (!canDraw)
        {
            return;
        }

        drawMachine.SetDraw();
        playableDirector.Play();

        SetAbleButton();

        haveMoney -= drawPrice;
        moneyText.text = "Money: " + haveMoney;

        int randomIndex = Random.Range(0, skinList.Count);

        newSkin = skinList[randomIndex];

        SkinManager.Instance.OpenSlime(newSkin);
       

        GameData gameData = SaveManager.Instance.LoadPlayerData();
        gameData.money = haveMoney;
        SaveManager.Instance.SavePlayerData(gameData);
    }

    private IEnumerator ShowSkin()
    {
        capsuleDirector.stopped -= OnTimelineStopped;
        Destroy(spawnedObject);

        GameObject skin = Instantiate(newSkin, new Vector3(0,3,-5),Quaternion.Euler(new Vector3(0,180,0)));
        skin.transform.localScale = new Vector3(2,2,2);
        skin.GetComponent<Rigidbody>().isKinematic = true;
        skin.GetComponent<Player>().enabled = false;

        skin.transform.Find("Shadow").gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);
        SetAbleButton(true);
        Destroy(skin );
    }

    public void OnTimelineStopped(PlayableDirector director)
    {
        StartCoroutine(ShowSkin());
    }

    public void SpawnObject()
    {
        if (prefab != null && spawnPoint != null)
        {
            spawnedObject = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation,spawnPoint);
            drawMachine.SetDraw();
            capsuleDirector = spawnedObject.GetComponent<PlayableDirector>();
            capsuleDirector.stopped += OnTimelineStopped;
        }
    }


    private void Exit()
    {
        SceneManager.LoadScene(2);
    }

    private void SetAbleButton(bool isInteract = false)
    {
        foreach(Button button in Buttons)
        {
            button.interactable = isInteract;
        }
    }
}
