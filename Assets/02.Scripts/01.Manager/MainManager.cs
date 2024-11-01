using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [TabGroup("UI"), LabelText("���� UI"), SerializeField]
    private GameObject mainUI;
    [TabGroup("UI"), LabelText("��Ų���� UI"), SerializeField]
    private GameObject skinUI;

    [TabGroup("UI", "��ư"), LabelText("���� ��ư"), SerializeField]
    private Button[] mainBtn;

    [TabGroup("UI", "����"), LabelText("�ְ� ���� text"), SerializeField]
    private TMP_Text bestScoreText;

    [TabGroup("UI", "��"), LabelText("�ְ� ���� text"), SerializeField]
    private TMP_Text moneyText;

    [TabGroup("UI", "��Ų"), LabelText("��Ų ����"), SerializeField]
    private RectTransform skinGroup;

    private List<GameObject> skins = new List<GameObject>();
    private float initialPositionX;
    private float scrollRange; // ��ũ�� ������ �ִ� ����
    private Vector2 lastMousePosition; // ���콺�� ������ ��ġ ����
    private bool isDragging = false; // �巡�� ������ ����

    private int skinIndex;

    [BoxGroup("���� ������"), LabelText("���� ��"), SerializeField]
    private uint money;
    [BoxGroup("���� ������"), LabelText("�ְ� ����"), SerializeField]
    private uint bestScore;

    private bool isOption;

    private void Start()
    {
        LoadData();

        mainUI.SetActive(true);

        InputManager.Instance.SwitchToActionMap("UI");

        // �� ��ư�� Ŭ�� �̺�Ʈ�� �ִϸ��̼� �߰�
        //foreach (Button btn in mainBtn)
        //{
        //    btn.onClick.AddListener(() => AnimateButton(btn));
        //}


        ArrangeSkins();

        // �ʱ� ��ġ�� ����
        initialPositionX = skinGroup.position.x;
        scrollRange = skins.Count * 40f; // ��Ų ������ ���� ��ũ�� ���� ����
    }
    private void Update()
    {
        if (skinUI.activeSelf)
        {
            Drag();

            if (isDragging)
            {
                ScrollSkinGroup();
            }
        }
    }

    #region button
    public void StartButton()  // ���� ���� ��ư
    {
        DisableButtons();

        //RectTransform btnRect1 = mainBtn[0].GetComponent<RectTransform>();
        //RectTransform btnRect2 = mainBtn[1].GetComponent<RectTransform>();
        //RectTransform btnRect3 = mainBtn[2].GetComponent<RectTransform>();

        //// UI�� ȭ�� ���� �ö� �� �Ʒ��� �������� �ִϸ��̼�
        //Vector2 belowScreenPos1 = new Vector2(btnRect1.anchoredPosition.x, -(Screen.height / 2 + btnRect1.rect.height)); // ȭ�� �Ʒ��� ���� ��ġ
        //Vector2 belowScreenPos2 = new Vector2(btnRect2.anchoredPosition.x, -(Screen.height / 2 + btnRect2.rect.height)); // ȭ�� �Ʒ��� ���� ��ġ
        //Vector2 belowScreenPos3 = new Vector2(btnRect3.anchoredPosition.x, -(Screen.height / 2 + btnRect3.rect.height)); // ȭ�� �Ʒ��� ���� ��ġ

        //// �ִϸ��̼� ������
        //Sequence startSequence = DOTween.Sequence();

        //// ȭ�� ���� ��¦ �ö󰬴ٰ� �Ʒ��� �������� �ִϸ��̼�
        //startSequence.Append(btnRect1.DOAnchorPos(belowScreenPos1, 0.6f).SetEase(Ease.InBack, 2)); // ȭ�� �Ʒ��� ������
        //startSequence.Insert(0.1f,btnRect2.DOAnchorPos(belowScreenPos2, 0.6f).SetEase(Ease.InBack, 2)); // ȭ�� �Ʒ��� ������
        //startSequence.Join(btnRect3.DOAnchorPos(belowScreenPos3, 0.6f).SetEase(Ease.InBack, 2)); // ȭ�� �Ʒ��� ������

        //// �ִϸ��̼� �Ϸ� �� Pause UI ��Ȱ��ȭ
        //startSequence.OnComplete(() =>
        //{

        RectTransform rectTransform = mainBtn[0].GetComponent<RectTransform>();

        mainBtn[0].interactable = false;
        // ��ư�� Y�� �������� ���� �ִϸ��̼� (0.5�� ����)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
        .OnComplete(() =>
        {
            EnableButtons();
            LoadingScene.LoadScene("03.GameScene");

        });
    }


    public void OptionButton() // ���� ��ư
    {
        DisableButtons();
        RectTransform rectTransform = mainBtn[1].GetComponent<RectTransform>();

        OptionManager.Instance.EnterOption();

        // ��ư�� Y�� �������� ���� �ִϸ��̼� (0.5�� ����)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
            .OnComplete(() =>
            {
                EnableButtons();
            });

        isOption = !isOption;
    }

    public void ExitButton()   // ���� ���� ��ư
    {
        DisableButtons();
        RectTransform rectTransform = mainBtn[2].GetComponent<RectTransform>();

        // ��ư�� Y�� �������� ���� �ִϸ��̼� (0.5�� ����)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
            .OnComplete(() =>
            {
                EnableButtons();
                Application.Quit();
            });

    }

    public void DrawBtn()
    {
        DisableButtons();

        RectTransform rectTransform = mainBtn[3].GetComponent<RectTransform>();

        // ��ư�� Y�� �������� ���� �ִϸ��̼� (0.5�� ����)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
        .OnComplete(() =>
        {
            EnableButtons();
            SceneManager.LoadScene("04.DrawScene");
        });
    }

    public void SkinBtn()
    {
        DisableButtons();

        RectTransform rectTransform = mainBtn[4].GetComponent<RectTransform>();


        // ��ư�� Y�� �������� ���� �ִϸ��̼� (0.5�� ����)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
        .OnComplete(() =>
        {
            EnableButtons();
            skinUI.SetActive(true);
        });
    }

    public void ExitSkinBtn()
    {
        skinUI.SetActive(false);
    }
    #endregion

    #region skin
    private void Drag()
    {
        // Check if skin UI is active and if mouse button is pressed, then start dragging
        if (InputManager.Instance.mousePressed)
        {
            if (!isDragging)
            {
                lastMousePosition = InputManager.Instance.mousePosition; // Initialize position on drag start
            }
            
            isDragging = true;

            
        }
        else
        {

            if (isDragging)
            {
                UpdateClosestSkin(); // �巡�� ���� �ÿ��� ���� ����� ��Ų ������Ʈ
                MoveSkinGroupToSelected(SkinManager.Instance.GetSelectSkin());
            }

            isDragging = false;  // Reset dragging when mouse is not pressed

        }
    }

    // ��Ų���� �ʱ� ��ġ�� ����
    private void ArrangeSkins()
    {
        for (int i = 0; i < SkinManager.Instance.GetOpenSlime().Count; i++)
        {
            GameObject skin = Instantiate(SkinManager.Instance.GetOpenSlime()[i], skinGroup.position,
                Quaternion.Euler(0, 180, 0), skinGroup);

            RectTransform rectTransform = skin.AddComponent<RectTransform>();
            rectTransform.position = skinGroup.position + new Vector3(i * 30, 0, -10);
            rectTransform.localScale *= 200;

            Renderer[] renderers = skin.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.SetFloat("_Surface", 1);
            }

            skin.GetComponent<Rigidbody>().isKinematic = true;
            skin.GetComponent<Player>().enabled = false;

            skin.transform.Find("Shadow").gameObject.SetActive(false);

            skins.Add(skin);
        }
    }

    private void ScrollSkinGroup()
    {
        Vector2 currentMousePosition = InputManager.Instance.mousePosition;

        Vector2 mouseDelta = lastMousePosition - currentMousePosition;

        // ��ũ�� �̵�
        float newPosX = skinGroup.position.x + mouseDelta.x * 0.1f; // �̵� �ӵ� ���� (0.1f�� �̵� �ӵ� ���)

        // ��ũ�� ����
        newPosX = Mathf.Clamp(newPosX, initialPositionX - scrollRange, initialPositionX);

        // ���ο� ��ġ ����
        skinGroup.position = new Vector3(newPosX, skinGroup.position.y, skinGroup.position.z);

        // ���콺 ��ġ ������Ʈ
        lastMousePosition = currentMousePosition;
    }

    private void UpdateClosestSkin()
    {

        float closestDistance = float.MaxValue;
        GameObject closestSkin = null;
      
        
        // �� ��Ų�� �߾� ��ġ�� �Ÿ� ���
        for (int i = 0; i < skins.Count; i++) 
        {
            float distance = Mathf.Abs(skins[i].transform.position.x);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSkin = skins[i];
                skinIndex = i;
            }
        }

        // ���� ����� ��Ų�� ����
        if (closestSkin != SkinManager.Instance.GetSelectSkin())
        {
            SkinManager.Instance.SelectSkin(closestSkin, skinIndex);
        }

    }

    private void MoveSkinGroupToSelected(GameObject selectedSkin)
    {
        skinGroup.DOAnchorPosX((skinIndex * -400f), 0.25f)
                 .SetEase(Ease.InQuad)
                 .OnComplete(() =>
                 {
                     skinGroup.anchoredPosition.Set((skinIndex * -400f), 0);
                     // �߰� �ڵ�: UpdateClosestSkin�� �ٽ� ȣ������ �ʵ��� �� ���� ���õ� ��Ų���� ����
                     SkinManager.Instance.SelectSkin(selectedSkin, skinIndex); // �̹� ���õ� ��Ų���� ����
                     isDragging = false;
                 });
    }
    #endregion

    // ����� ������ �ҷ�����
    private void LoadData()
    {
        GameData data = SaveManager.Instance.LoadPlayerData();
        
        bestScore = data.score;
        money = data.money;

        bestScoreText.text = bestScore.ToString();
        moneyText.text = money.ToString();
    }

    // ��ư Ŭ�� �� ��鸮�� �ִϸ��̼� ����
    private void AnimateButton(Button button)
    {
        RectTransform rectTransform = button.GetComponent<RectTransform>();

        button.interactable = false;

        // ��ư�� Y�� �������� ���� �ִϸ��̼� (0.5�� ����)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
            .OnComplete(() =>
            {
                button.interactable = true;
            });
    }

    // ��� ��ư ��Ȱ��ȭ
    private void DisableButtons()
    {
        foreach (Button btn in mainBtn)
        {
            btn.interactable = false;
        }
    }

    private void EnableButtons()
    {
        foreach (Button btn in mainBtn)
        {
            btn.interactable = true;
        }
    }



}
