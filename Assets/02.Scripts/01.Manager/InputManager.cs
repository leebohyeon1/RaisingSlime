using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>, IUpdateable
{
    private PlayerInput playerInput; // 플레이어 InputSystem

    // 타이틀 씬 액션맵
    private InputAction anyKeyAction;

    public bool anyKeyInput {  get; private set; }


    // 게임 씬 액션맵
    private InputAction moveAction; 
    private InputAction jumpAction;

    public Vector2 moveInput { get; private set; } // 이동 벡터 값
    public bool jumpInput { get; private set; }    // 점프 값

    protected override void Awake()
    {
        base.Awake();

        playerInput = GetComponent<PlayerInput>();

        SetUpInputActions();
    }

    protected override void Start()
    {
        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    public virtual void OnUpdate(float dt)
    {
        HandleActions();
    }

    private void SetUpInputActions() // ActionMap의 InputAction 초기화
    {
        moveAction = playerInput.actions["Move"];     
        jumpAction = playerInput.actions["Jump"];
        anyKeyAction = playerInput.actions["AnyKey"];
    }

    private void HandleActions() // 액션 값 받는 함수
    {
        moveInput = moveAction.ReadValue<Vector2>();  // moveAction 값 초기화

        jumpInput = jumpAction.WasPressedThisFrame(); // jumpAction 값 초기화 

        anyKeyInput = anyKeyAction.WasPressedThisFrame();   
    }

    // 디폴트 맵을 변경하는 함수
    public void SwitchToActionMap(string actionMapName)
    {
        if (playerInput != null)
        {
            // 현재 맵을 변경
            playerInput.SwitchCurrentActionMap(actionMapName);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (GameLogicManager.Instance != null)
        {
            GameLogicManager.Instance.DeregisterUpdatableObject(this);
        }
    }
}
