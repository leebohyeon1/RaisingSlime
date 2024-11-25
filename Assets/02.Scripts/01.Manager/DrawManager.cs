using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DrawManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private uint drawPrice;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private Button[] buttons;
    [SerializeField] private GameObject[] prefabs; // Capsule, Coin prefabs
    [SerializeField] private Transform[] spawnPoints; // Capsule, Coin spawn points
    [SerializeField] private DrawMachine drawMachine;
    [SerializeField] private PlayableDirector mainPlayableDirector;

    private uint haveMoney;
    private GameObject newSkin;
    private GameObject spawnedCapsule;
    private PlayableDirector capsuleDirector;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Load player data
        var gameData = SaveManager.Instance.LoadPlayerData();
        haveMoney = gameData.money;
        UpdateMoneyText();

        // Assign button listeners
        buttons[0].onClick.AddListener(() => Draw());
        buttons[1].onClick.AddListener(Exit);
    }

    private void Draw()
    {
        var skinList = SkinManager.Instance.GetCloseSlime();

        if (haveMoney < drawPrice || skinList.Count == 0)
            return;

        // Deduct money and start draw sequence
        haveMoney -= drawPrice;
        UpdateMoneyText();

        drawMachine.SetDraw();
        mainPlayableDirector.Play();
        SetButtonsInteractable(false);

        // Select and unlock a random skin
        newSkin = skinList[Random.Range(0, skinList.Count)];
        SkinManager.Instance.OpenSlime(newSkin);

        // Save updated data
        var gameData = SaveManager.Instance.LoadPlayerData();
        gameData.money = haveMoney;
        SaveManager.Instance.SavePlayerData(gameData);
    }

    private IEnumerator ShowSkin()
    {
        capsuleDirector.stopped -= OnTimelineStopped;
        Destroy(spawnedCapsule);

        // Instantiate and configure skin
        var skin = Instantiate(newSkin, new Vector3(0, 3, -5), Quaternion.Euler(0, 180, 0));
        skin.transform.localScale = Vector3.one * 2;
        DisableSkinComponents(skin);

        // Smooth rotation animation
        float rotationTime = 2f;
        float totalRotation = 1435f;
        float elapsedTime = 0f;

        while (elapsedTime < rotationTime)
        {
            float progress = elapsedTime / rotationTime;
            float slowDownFactor = Mathf.Lerp(1f, 0f, progress);
            float deltaRotation = (totalRotation / rotationTime) * Time.deltaTime * slowDownFactor;
            skin.transform.Rotate(0, deltaRotation, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        skin.transform.rotation = Quaternion.Euler(0, 180, 0);
        yield return new WaitForSeconds(1f);

        // Reduce skin scale with DOTween
        float scaleDuration = 0.5f;
        skin.transform.DOScale(Vector3.zero, scaleDuration).OnComplete(() =>
        {
            Destroy(skin);
        });


        // Wait for the scale animation to complete
        yield return new WaitForSeconds(scaleDuration);

        SetButtonsInteractable(true);
    }

    private void DisableSkinComponents(GameObject skin)
    {
        var rb = skin.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        var player = skin.GetComponent<Player>();
        if (player != null) player.enabled = false;

        var shadow = skin.transform.Find("Shadow");
        if (shadow != null) shadow.gameObject.SetActive(false);
    }

    public void OnTimelineStopped(PlayableDirector director)
    {
        StartCoroutine(ShowSkin());
    }

    public void SpawnCapsule()
    {
        if (prefabs[0] == null || spawnPoints[0] == null)
            return;

        spawnedCapsule = Instantiate(prefabs[Random.Range(0,4)], spawnPoints[0].position, spawnPoints[0].rotation, spawnPoints[0]);
        drawMachine.SetDraw();
        capsuleDirector = spawnedCapsule.GetComponent<PlayableDirector>();
        capsuleDirector.stopped += OnTimelineStopped;
    }

    public void SpawnCoin()
    {
        if (prefabs[4] == null || spawnPoints[1] == null)
            return;

        var coin = Instantiate(prefabs[4], spawnPoints[1].position, spawnPoints[1].rotation, spawnPoints[1]);
        Destroy(coin, 1.5f);
    }

    private void Exit()
    {
        SceneManager.LoadScene(2);
    }

    private void UpdateMoneyText()
    {
        moneyText.text = haveMoney.ToString();
    }

    private void SetButtonsInteractable(bool isInteractable)
    {
        foreach (var button in buttons)
        {
            button.interactable = isInteractable;
        }
    }
}
