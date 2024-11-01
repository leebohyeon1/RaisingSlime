using DG.Tweening;
using Sirenix.OdinInspector;
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

    [TabGroup("UI", "점수"), LabelText("최고 점수 text"), SerializeField]
    private TMP_Text bestScoreText;

    [TabGroup("UI", "돈"), LabelText("최고 점수 text"), SerializeField]
    private TMP_Text moneyText;

    [TabGroup("UI", "스킨"), LabelText("스킨 모음"), SerializeField]
    private RectTransform skinGroup;

    private List<GameObject> skins = new List<GameObject>();
    private float initialPositionX;
    private float scrollRange; // 스크롤 가능한 최대 범위
    private Vector2 lastMousePosition; // 마우스의 마지막 위치 저장
    private bool isDragging = false; // 드래그 중인지 여부

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

        // 각 버튼에 클릭 이벤트와 애니메이션 추가
        //foreach (Button btn in mainBtn)
        //{
        //    btn.onClick.AddListener(() => AnimateButton(btn));
        //}


        ArrangeSkins();

        // 초기 위치를 저장
        initialPositionX = skinGroup.position.x;
        scrollRange = skins.Count * 40f; // 스킨 개수에 따라 스크롤 범위 설정
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
    public void StartButton()  // 게임 시작 버튼
    {
        DisableButtons();

        //RectTransform btnRect1 = mainBtn[0].GetComponent<RectTransform>();
        //RectTransform btnRect2 = mainBtn[1].GetComponent<RectTransform>();
        //RectTransform btnRect3 = mainBtn[2].GetComponent<RectTransform>();

        //// UI가 화면 위로 올라간 후 아래로 떨어지는 애니메이션
        //Vector2 belowScreenPos1 = new Vector2(btnRect1.anchoredPosition.x, -(Screen.height / 2 + btnRect1.rect.height)); // 화면 아래로 임의 위치
        //Vector2 belowScreenPos2 = new Vector2(btnRect2.anchoredPosition.x, -(Screen.height / 2 + btnRect2.rect.height)); // 화면 아래로 임의 위치
        //Vector2 belowScreenPos3 = new Vector2(btnRect3.anchoredPosition.x, -(Screen.height / 2 + btnRect3.rect.height)); // 화면 아래로 임의 위치

        //// 애니메이션 시퀀스
        //Sequence startSequence = DOTween.Sequence();

        //// 화면 위로 살짝 올라갔다가 아래로 떨어지는 애니메이션
        //startSequence.Append(btnRect1.DOAnchorPos(belowScreenPos1, 0.6f).SetEase(Ease.InBack, 2)); // 화면 아래로 떨어짐
        //startSequence.Insert(0.1f,btnRect2.DOAnchorPos(belowScreenPos2, 0.6f).SetEase(Ease.InBack, 2)); // 화면 아래로 떨어짐
        //startSequence.Join(btnRect3.DOAnchorPos(belowScreenPos3, 0.6f).SetEase(Ease.InBack, 2)); // 화면 아래로 떨어짐

        //// 애니메이션 완료 후 Pause UI 비활성화
        //startSequence.OnComplete(() =>
        //{

        RectTransform rectTransform = mainBtn[0].GetComponent<RectTransform>();

        mainBtn[0].interactable = false;
        // 버튼을 Y축 방향으로 흔드는 애니메이션 (0.5초 동안)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
        .OnComplete(() =>
        {
            EnableButtons();
            LoadingScene.LoadScene("03.GameScene");

        });
    }


    public void OptionButton() // 설정 버튼
    {
        DisableButtons();
        RectTransform rectTransform = mainBtn[1].GetComponent<RectTransform>();

        OptionManager.Instance.EnterOption();

        // 버튼을 Y축 방향으로 흔드는 애니메이션 (0.5초 동안)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
            .OnComplete(() =>
            {
                EnableButtons();
            });

        isOption = !isOption;
    }

    public void ExitButton()   // 게임 종료 버튼
    {
        DisableButtons();
        RectTransform rectTransform = mainBtn[2].GetComponent<RectTransform>();

        // 버튼을 Y축 방향으로 흔드는 애니메이션 (0.5초 동안)
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

        // 버튼을 Y축 방향으로 흔드는 애니메이션 (0.5초 동안)
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


        // 버튼을 Y축 방향으로 흔드는 애니메이션 (0.5초 동안)
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
                UpdateClosestSkin(); // 드래그 시작 시에만 가장 가까운 스킨 업데이트
                MoveSkinGroupToSelected(SkinManager.Instance.GetSelectSkin());
            }

            isDragging = false;  // Reset dragging when mouse is not pressed

        }
    }

    // 스킨들의 초기 위치를 설정
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

        // 스크롤 이동
        float newPosX = skinGroup.position.x + mouseDelta.x * 0.1f; // 이동 속도 조정 (0.1f는 이동 속도 계수)

        // 스크롤 제한
        newPosX = Mathf.Clamp(newPosX, initialPositionX - scrollRange, initialPositionX);

        // 새로운 위치 설정
        skinGroup.position = new Vector3(newPosX, skinGroup.position.y, skinGroup.position.z);

        // 마우스 위치 업데이트
        lastMousePosition = currentMousePosition;
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
                     isDragging = false;
                 });
    }
    #endregion

    // 저장된 데이터 불러오기
    private void LoadData()
    {
        GameData data = SaveManager.Instance.LoadPlayerData();
        
        bestScore = data.score;
        money = data.money;

        bestScoreText.text = bestScore.ToString();
        moneyText.text = money.ToString();
    }

    // 버튼 클릭 시 흔들리는 애니메이션 적용
    private void AnimateButton(Button button)
    {
        RectTransform rectTransform = button.GetComponent<RectTransform>();

        button.interactable = false;

        // 버튼을 Y축 방향으로 흔드는 애니메이션 (0.5초 동안)
        rectTransform.DOShakeAnchorPos(0.5f, new Vector3(0f, 10f, 0f), 8, 0, false, true)
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
