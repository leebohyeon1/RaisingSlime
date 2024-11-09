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
    [TabGroup("UI"), LabelText("메인 UI"), SerializeField]
    private GameObject mainUI;
    [TabGroup("UI"), LabelText("스킨선택 UI"), SerializeField]
    private GameObject skinUI;

    [TabGroup("UI", "버튼"), LabelText("메인 버튼"), SerializeField]
    private Button[] mainBtn;
    private Vector3[] mainBtnDefaultScale;
    [TabGroup("UI", "점수"), LabelText("최고 점수 text"), SerializeField]
    private TMP_Text bestScoreText;

    [TabGroup("UI", "돈"), LabelText("최고 점수 text"), SerializeField]
    private TMP_Text moneyText;

    [TabGroup("UI", "스킨"), LabelText("스킨 모음"), SerializeField]
    private RectTransform skinGroup;
    [TabGroup("UI", "스킨"), LabelText("스킨 이름"), SerializeField]
    private TMP_Text skinNameText;
    [TabGroup("UI", "스킨"), LabelText("스킨 보유 유무"), SerializeField]
    private GameObject[] skinProssession;
    [TabGroup("UI", "버튼"), LabelText("스킨 버튼"), SerializeField]
    private Button[] skinBtn;
    private Vector3[] skinBtnDefaultScale;

    private List<GameObject> skins = new List<GameObject>();
    private List<Vector3> defaultSkinScale = new List<Vector3>();
    private Vector2 skinUiOriginalPos;
    private Vector2 skinBtnOriginalPos;


    private float initialPositionX;
    private float scrollRange; // 스크롤 가능한 최대 범위
    private Vector2 lastMousePosition; // 마우스의 마지막 위치 저장

    private bool isDragging = false; // 드래그 중인지 여부
    private bool isSkinBtnMoving = false;
    private bool canDrag = false;

    private int skinIndex;

    [BoxGroup("저장 데이터"), LabelText("보유 돈"), SerializeField]
    private uint money;
    [BoxGroup("저장 데이터"), LabelText("최고 점수"), SerializeField]
    private uint bestScore;

    private bool isOption;

    private void Start()
    {
        LoadData();

        mainUI.SetActive(true);

        InputManager.Instance.SwitchToActionMap("UI");

        ArrangeSkins();

        // 초기 위치를 저장
        initialPositionX = skinGroup.position.x;
        scrollRange = skins.Count * 40f; // 스킨 개수에 따라 스크롤 범위 설정

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
    public void StartButton() // 게임 시작 버튼
    {
        ExecuteButtonAction(mainBtn[0], mainBtnDefaultScale[0], () =>
        {
            LoadingScene.LoadScene("03.GameScene");
        });
    }

    public void OptionButton() // 설정 버튼
    {
        ExecuteButtonAction(mainBtn[1], mainBtnDefaultScale[1], () =>
        {
            OptionManager.Instance.EnterOption();
            isOption = !isOption;
        });
    }

    public void ExitButton() // 게임 종료 버튼
    {
        ExecuteButtonAction(mainBtn[2], mainBtnDefaultScale[2], Application.Quit);
    }

    public void DrawBtn() // 그림 씬 로드 버튼
    {
        ExecuteButtonAction(mainBtn[3], mainBtnDefaultScale[3], () =>
        {
            SceneManager.LoadScene("04.DrawScene");
        });
    }

    public void SkinBtn() // 스킨 UI 열기 버튼
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

            Vector2 offScreenPos = new Vector2(optionRect.anchoredPosition.x, Screen.height / 2 + optionRect.rect.height); // 화면 위의 임의 위치
            Vector2 btnOffScreenPos = new Vector2(btnRect.anchoredPosition.x, Screen.height / 2 + btnRect.rect.height); // 화면 위의 임의 위치

            optionRect.anchoredPosition = offScreenPos;
            btnRect.anchoredPosition = btnOffScreenPos;

            Sequence enterOptionSequence = DOTween.Sequence();
            
            enterOptionSequence.Append(btnRect.DOAnchorPos(skinBtnOriginalPos, 0.8f).SetEase(Ease.OutElastic, 1.2f, 0.6f));
            enterOptionSequence.Insert(0.1f, optionRect.DOAnchorPos(skinUiOriginalPos, 0.6f).SetEase(Ease.OutElastic, 1.2f, 0.6f).OnComplete(() => { MoveSkinGroupToCurSkin(); }));


            enterOptionSequence.SetUpdate(true);
        }
    }

    public void ExitSkinBtn() // 스킨 UI 닫기 버튼
    {
        AudioManager.Instance.PlaySFX("Btn");
        skinBtn[2].interactable = false;

        if (skinUI.activeSelf)
        {
            RectTransform optionRect = skinUI.GetComponent<RectTransform>();
            RectTransform btnRect = skinBtn[2].GetComponent<RectTransform>();

            // 화면 아래 위치 계산 (공통 코드로 이동)
            Vector2 belowScreenPos = new Vector2(optionRect.anchoredPosition.x, -(Screen.height / 2 + optionRect.rect.height));
            Vector2 btnBelowScreenPos = new Vector2(btnRect.anchoredPosition.x, belowScreenPos.y);

            // 애니메이션 시퀀스
            Sequence exitOptionSequence = DOTween.Sequence();

            exitOptionSequence.Append(btnRect.DOAnchorPos(btnBelowScreenPos, 0.6f).SetEase(Ease.InBack, 0.5f));
            exitOptionSequence.Insert(0.1f, optionRect.DOAnchorPos(belowScreenPos, 0.6f).SetEase(Ease.InBack, 1.7f));

            // 애니메이션 완료 후, 원래 자리로 복귀하고 비활성화
            exitOptionSequence.OnComplete(() =>
            {
                // 위치를 즉시 원래 자리로 설정
                optionRect.anchoredPosition = skinUiOriginalPos;
                btnRect.anchoredPosition = skinBtnOriginalPos;

                skinUI.SetActive(false);
            });

            // 타임 스케일에 상관없이 애니메이션이 작동하도록 설정
            exitOptionSequence.SetUpdate(true);
   
        }
    }


    // 공통 애니메이션 및 액션 실행 함수
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
                UpdateClosestSkin(); // 드래그 시작 시에만 가장 가까운 스킨 업데이트
                MoveSkinGroupToSelected(SkinManager.Instance.GetSelectSkin());
            }

            isDragging = false;  

        }
    }

    // 스킨들의 초기 위치를 설정
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

        // 스크롤 이동
        float newPosX = skinGroup.position.x + mouseDelta.x * 0.1f; // 이동 속도 조정 (0.1f는 이동 속도 계수)

        // 스크롤 제한
        newPosX = Mathf.Clamp(newPosX, initialPositionX - scrollRange, initialPositionX);

        // 새로운 위치 설정
        skinGroup.position = new Vector3(newPosX, skinGroup.position.y, skinGroup.position.z);

        // 마우스 위치 업데이트
        lastMousePosition = currentMousePosition;

        float closestDistance = float.MaxValue;

        // 각 스킨과 중앙 위치의 거리 계산
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
      
        
        // 각 스킨과 중앙 위치의 거리 계산
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

        // 가장 가까운 스킨을 선택
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
                     // 추가 코드: UpdateClosestSkin을 다시 호출하지 않도록 한 번만 선택된 스킨으로 고정                  
                     SkinManager.Instance.SelectSkin(selectedSkin, skinIndex); // 이미 선택된 스킨으로 고정
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
                     // 추가 코드: UpdateClosestSkin을 다시 호출하지 않도록 한 번만 선택된 스킨으로 고정                  
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

    // 저장된 데이터 불러오기
    private void LoadData()
    {
        GameData data = SaveManager.Instance.LoadPlayerData();
        
        bestScore = data.score;
        money = data.money;
        skinIndex = data.curPlayerIndex;

        bestScoreText.text = bestScore.ToString();
        moneyText.text = money.ToString();


    }

    // 버튼 클릭 시 흔들리는 애니메이션 적용
    private void AnimateButton(Button button)
    {
        RectTransform rectTransform = button.GetComponent<RectTransform>();

        button.interactable = false;

        // 버튼을 Y축 방향으로 흔드는 애니메이션 (0.5초 동안)
        rectTransform.DOPunchScale(mainBtn[0].transform.localScale, 0.2f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                button.interactable = true;
            });
    }

    // 모든 버튼 비활성화
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
