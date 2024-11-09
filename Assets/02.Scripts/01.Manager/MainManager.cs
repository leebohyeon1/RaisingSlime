using DG.Tweening;
using Sirenix.OdinInspector;
using System;
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
    private Vector3[] mainBtnDefaultScale;
    [TabGroup("UI", "����"), LabelText("�ְ� ���� text"), SerializeField]
    private TMP_Text bestScoreText;

    [TabGroup("UI", "��"), LabelText("�ְ� ���� text"), SerializeField]
    private TMP_Text moneyText;

    [TabGroup("UI", "��Ų"), LabelText("��Ų ����"), SerializeField]
    private RectTransform skinGroup;
    [TabGroup("UI", "��Ų"), LabelText("��Ų �̸�"), SerializeField]
    private TMP_Text skinNameText;
    [TabGroup("UI", "��Ų"), LabelText("��Ų ���� ����"), SerializeField]
    private GameObject[] skinProssession;
    [TabGroup("UI", "��ư"), LabelText("��Ų ��ư"), SerializeField]
    private Button[] skinBtn;
    private Vector3[] skinBtnDefaultScale;

    private List<GameObject> skins = new List<GameObject>();
    private List<Vector3> defaultSkinScale = new List<Vector3>();
    private Vector2 skinUiOriginalPos;
    private Vector2 skinBtnOriginalPos;


    private float initialPositionX;
    private float scrollRange; // ��ũ�� ������ �ִ� ����
    private Vector2 lastMousePosition; // ���콺�� ������ ��ġ ����

    private bool isDragging = false; // �巡�� ������ ����
    private bool isSkinBtnMoving = false;
    private bool canDrag = false;

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

        ArrangeSkins();

        // �ʱ� ��ġ�� ����
        initialPositionX = skinGroup.position.x;
        scrollRange = skins.Count * 40f; // ��Ų ������ ���� ��ũ�� ���� ����

        skinBtn[0].onClick.AddListener(() => MoveSkinGroupToRight());
        skinBtn[1].onClick.AddListener(() => MoveSkinGroupToLeft());

        if(!AudioManager.Instance.CheckCurBGM("TitleBGM"))
        {
            AudioManager.Instance.PlayBGM("TitleBGM");
        }

        mainBtnDefaultScale = new Vector3[mainBtn.Length];
        for (int i = 0; i < mainBtn.Length; i++)
        {
            mainBtnDefaultScale[i] = mainBtn[i].transform.localScale;
        }

        skinBtnDefaultScale = new Vector3[skinBtn.Length];
        for(int i = 0;i < skinBtn.Length; i++)
        {
            skinBtnDefaultScale[i] = skinBtn[i].transform.localScale;
        }

        skinUiOriginalPos = skinUI.GetComponent<RectTransform>().anchoredPosition;
        skinBtnOriginalPos = skinBtn[2].GetComponent<RectTransform>().anchoredPosition;
    }

    private void Update()
    {
        if (skinUI.activeSelf)
        {
            Drag();

            ChangeSkinScale();

            if (isDragging)
            {
                ScrollSkinGroup();
            }
        }
    }

    #region button
    public void StartButton() // ���� ���� ��ư
    {
        ExecuteButtonAction(mainBtn[0], mainBtnDefaultScale[0], () =>
        {
            LoadingScene.LoadScene("03.GameScene");
        });
    }

    public void OptionButton() // ���� ��ư
    {
        ExecuteButtonAction(mainBtn[1], mainBtnDefaultScale[1], () =>
        {
            OptionManager.Instance.EnterOption();
            isOption = !isOption;
        });
    }

    public void ExitButton() // ���� ���� ��ư
    {
        ExecuteButtonAction(mainBtn[2], mainBtnDefaultScale[2], Application.Quit);
    }

    public void DrawBtn() // �׸� �� �ε� ��ư
    {
        ExecuteButtonAction(mainBtn[3], mainBtnDefaultScale[3], () =>
        {
            SceneManager.LoadScene("04.DrawScene");
        });
    }

    public void SkinBtn() // ��Ų UI ���� ��ư
    {

        AudioManager.Instance.PlaySFX("Btn");

        skinBtn[2].GetComponent<Button>().interactable = true;
        if (!skinUI.activeSelf)
        {
            RectTransform optionRect = skinUI.GetComponent<RectTransform>();
            RectTransform btnRect = skinBtn[2].GetComponent<RectTransform>();

            optionRect.anchoredPosition = skinUiOriginalPos;
            btnRect.anchoredPosition = skinBtnOriginalPos;

            skinUI.SetActive(true);

            Vector2 offScreenPos = new Vector2(optionRect.anchoredPosition.x, Screen.height / 2 + optionRect.rect.height); // ȭ�� ���� ���� ��ġ
            Vector2 btnOffScreenPos = new Vector2(btnRect.anchoredPosition.x, Screen.height / 2 + btnRect.rect.height); // ȭ�� ���� ���� ��ġ

            optionRect.anchoredPosition = offScreenPos;
            btnRect.anchoredPosition = btnOffScreenPos;

            Sequence enterOptionSequence = DOTween.Sequence();
            
            enterOptionSequence.Append(btnRect.DOAnchorPos(skinBtnOriginalPos, 0.8f).SetEase(Ease.OutElastic, 1.2f, 0.6f));
            enterOptionSequence.Insert(0.1f, optionRect.DOAnchorPos(skinUiOriginalPos, 0.6f).SetEase(Ease.OutElastic, 1.2f, 0.6f).OnComplete(() => { MoveSkinGroupToCurSkin(); }));


            enterOptionSequence.SetUpdate(true);
        }
    }

    public void ExitSkinBtn() // ��Ų UI �ݱ� ��ư
    {
        AudioManager.Instance.PlaySFX("Btn");
        skinBtn[2].interactable = false;

        if (skinUI.activeSelf)
        {
            RectTransform optionRect = skinUI.GetComponent<RectTransform>();
            RectTransform btnRect = skinBtn[2].GetComponent<RectTransform>();

            // ȭ�� �Ʒ� ��ġ ��� (���� �ڵ�� �̵�)
            Vector2 belowScreenPos = new Vector2(optionRect.anchoredPosition.x, -(Screen.height / 2 + optionRect.rect.height));
            Vector2 btnBelowScreenPos = new Vector2(btnRect.anchoredPosition.x, belowScreenPos.y);

            // �ִϸ��̼� ������
            Sequence exitOptionSequence = DOTween.Sequence();

            exitOptionSequence.Append(btnRect.DOAnchorPos(btnBelowScreenPos, 0.6f).SetEase(Ease.InBack, 0.5f));
            exitOptionSequence.Insert(0.1f, optionRect.DOAnchorPos(belowScreenPos, 0.6f).SetEase(Ease.InBack, 1.7f));

            // �ִϸ��̼� �Ϸ� ��, ���� �ڸ��� �����ϰ� ��Ȱ��ȭ
            exitOptionSequence.OnComplete(() =>
            {
                // ��ġ�� ��� ���� �ڸ��� ����
                optionRect.anchoredPosition = skinUiOriginalPos;
                btnRect.anchoredPosition = skinBtnOriginalPos;

                skinUI.SetActive(false);
            });

            // Ÿ�� �����Ͽ� ������� �ִϸ��̼��� �۵��ϵ��� ����
            exitOptionSequence.SetUpdate(true);
   
        }
    }


    // ���� �ִϸ��̼� �� �׼� ���� �Լ�
    private void ExecuteButtonAction(Button button, Vector3 defaultScale, Action action)
    {
        DisableButtons();
        AudioManager.Instance.PlaySFX("Btn");

        RectTransform rectTransform = button.GetComponent<RectTransform>();
        button.interactable = false;

        rectTransform.DOScale(defaultScale / 2, 0.1f)
            .OnComplete(() =>
            {
                rectTransform.DOScale(defaultScale, 0.1f).OnComplete(() =>
                {
                    EnableButtons();
                    button.interactable = true;
                    action?.Invoke();
                });
            });
    }
    #endregion

    #region skin
    private void Drag()
    {
        if (!canDrag && !isDragging)
        {
            return;
        }

        if (InputManager.Instance.mousePressed)
        {
            isSkinBtnMoving = false;
            DOTween.Kill(skinGroup);


            if (!isSkinBtnMoving)
            {
                if (!isDragging)
                {
                    lastMousePosition = InputManager.Instance.mousePosition; 
                }

                isDragging = true;
            }
            
        }
        else
        {

            if (isDragging && !isSkinBtnMoving)
            {
                UpdateClosestSkin(); // �巡�� ���� �ÿ��� ���� ����� ��Ų ������Ʈ
                MoveSkinGroupToSelected(SkinManager.Instance.GetSelectSkin());
            }

            isDragging = false;  

        }
    }

    // ��Ų���� �ʱ� ��ġ�� ����
    private void ArrangeSkins()
    {
        for (int i = 0; i < SkinManager.Instance.GetSkinCount(); i++)
        {
            GameObject skin = Instantiate(SkinManager.Instance.GetSkinList()[i], skinGroup.position,
                Quaternion.Euler(0, 180, 0), skinGroup);

            RectTransform rectTransform = skin.AddComponent<RectTransform>();
            rectTransform.position = skinGroup.position + new Vector3(i * 30, 0, -10);
            rectTransform.localScale *= 350;

            Renderer[] renderers = skin.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material.SetFloat("_Surface", 1);
            }

            skin.GetComponent<Rigidbody>().isKinematic = true;
            skin.GetComponent<Player>().enabled = false;

            skin.transform.Find("Shadow").gameObject.SetActive(false);

            skins.Add(skin);
            defaultSkinScale.Add(skin.transform.localScale);
        }

        ChangeSkinScale();

        MoveSkinGroupToCurSkin();

        //ChangeSkinProssession(); 

    }

    private void ScrollSkinGroup()
    {
        Vector2 currentMousePosition = InputManager.Instance.mousePosition;

        Vector2 mouseDelta =  currentMousePosition - lastMousePosition;

        // ��ũ�� �̵�
        float newPosX = skinGroup.position.x + mouseDelta.x * 0.1f; // �̵� �ӵ� ���� (0.1f�� �̵� �ӵ� ���)

        // ��ũ�� ����
        newPosX = Mathf.Clamp(newPosX, initialPositionX - scrollRange, initialPositionX);

        // ���ο� ��ġ ����
        skinGroup.position = new Vector3(newPosX, skinGroup.position.y, skinGroup.position.z);

        // ���콺 ��ġ ������Ʈ
        lastMousePosition = currentMousePosition;

        float closestDistance = float.MaxValue;

        // �� ��Ų�� �߾� ��ġ�� �Ÿ� ���
        for (int i = 0; i < skins.Count; i++)
        {
            float distance = Mathf.Abs(skins[i].transform.position.x);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                skinIndex = i;
            }

            ChangeSkinProssession();
        }

        skinNameText.text = SkinManager.Instance.GetSkinName(skinIndex);
    }

    private void MoveSkinGroupToRight()
    {
        if (skinIndex >= skins.Count - 1 || isSkinBtnMoving)
        {
            return;
        }

        DOTween.Kill(skinGroup);


        isSkinBtnMoving = true;

        skinIndex += 1;

        skinGroup.DOAnchorPosX((skinIndex * -400f), 0.4f).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                isSkinBtnMoving = false;
                UpdateClosestSkin();
                ChangeSkinProssession();
            });


        ExecuteButtonAction(skinBtn[0], skinBtnDefaultScale[0], () => { });
    }

    private void MoveSkinGroupToLeft()
    {
        if (skinIndex <= 0 || isSkinBtnMoving)
        {
            return;
        }

        DOTween.Kill(skinGroup);

  
        isSkinBtnMoving = true;

        skinIndex -= 1;
        skinGroup.DOAnchorPosX((skinIndex * -400f), 0.4f).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                isSkinBtnMoving = false;
                UpdateClosestSkin();
                ChangeSkinProssession();
            });

        ExecuteButtonAction(skinBtn[1], skinBtnDefaultScale[1], () => { });
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
            skinNameText.text = SkinManager.Instance.GetCurSkinName();
            ChangeSkinProssession();
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
                     skinNameText.text = SkinManager.Instance.GetCurSkinName();

                     isDragging = false;
                 });
    }

    private void MoveSkinGroupToCurSkin()
    {
        skinIndex = SkinManager.Instance.GetPlayerIndex();

        skinGroup.DOAnchorPosX((SkinManager.Instance.GetPlayerIndex() * -400f), 0.25f)
                 .SetEase(Ease.InQuad)
                 .OnComplete(() =>
                 {
                     skinGroup.anchoredPosition.Set((SkinManager.Instance.GetPlayerIndex() * -400f), 0);
                     // �߰� �ڵ�: UpdateClosestSkin�� �ٽ� ȣ������ �ʵ��� �� ���� ���õ� ��Ų���� ����                  
                     skinNameText.text = SkinManager.Instance.GetPlayerName();
                 
                     isDragging = false;

                     ChangeSkinProssession();
                 });
    }

    public void OnDrag(bool cnaDrag)
    {
        canDrag = cnaDrag;
    }

    private void ChangeSkinScale()
    {
        for (int i = 0; i < skins.Count; i++)
        {
            float distance = Vector2.Distance(Vector2.zero, new Vector2(skins[i].transform.position.x, 0));

            Vector3 scale =  defaultSkinScale[i] - (new Vector3(distance, distance, distance) * 1.5f);
            skins[i].transform.localScale = scale;
        }
    }

    private void ChangeSkinProssession()
    {

        if (skinIndex == SkinManager.Instance.GetPlayerIndex() && SkinManager.Instance.IsSlimeOpen(skinIndex))
        {
            skinProssession[0].SetActive(true);
            skinProssession[1].SetActive(false);
            skinProssession[2].SetActive(false);
        }
        else if (SkinManager.Instance.IsSlimeOpen(skinIndex))
        {
            skinProssession[0].SetActive(false);
            skinProssession[1].SetActive(true);
            skinProssession[2].SetActive(false);
        }
        else
        {
            skinProssession[0].SetActive(false);
            skinProssession[1].SetActive(false);
            skinProssession[2].SetActive(true);
        }
    }
    #endregion

    // ����� ������ �ҷ�����
    private void LoadData()
    {
        GameData data = SaveManager.Instance.LoadPlayerData();
        
        bestScore = data.score;
        money = data.money;
        skinIndex = data.curPlayerIndex;

        bestScoreText.text = bestScore.ToString();
        moneyText.text = money.ToString();


    }

    // ��ư Ŭ�� �� ��鸮�� �ִϸ��̼� ����
    private void AnimateButton(Button button)
    {
        RectTransform rectTransform = button.GetComponent<RectTransform>();

        button.interactable = false;

        // ��ư�� Y�� �������� ���� �ִϸ��̼� (0.5�� ����)
        rectTransform.DOPunchScale(mainBtn[0].transform.localScale, 0.2f).SetEase(Ease.InBack)
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
