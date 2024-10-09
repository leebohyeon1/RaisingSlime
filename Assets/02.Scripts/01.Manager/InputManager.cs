using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, IUpdateable
{
    public static InputManager Instance { get; private set; } // �̱��� ����


    private PlayerInput playerInput; // �÷��̾� InputSystem

    // Ÿ��Ʋ �� �׼Ǹ�
    private InputAction anyKeyAction;

    public bool anyKeyInput {  get; private set; }


    // ���� �� �׼Ǹ�
    private InputAction moveAction; 
    private InputAction jumpAction;

    public Vector2 moveInput { get; private set; } // �̵� ���� ��
    public bool jumpInput { get; private set; }    // ���� ��

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }  
    }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        SetUpInputActions();

        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    public virtual void OnUpdate(float dt)
    {
        HandleActions();
    }

    private void SetUpInputActions() // ActionMap�� InputAction �ʱ�ȭ
    {
        moveAction = playerInput.actions["Move"];     
        jumpAction = playerInput.actions["Jump"];
        anyKeyAction = playerInput.actions["AnyKey"];
    }

    private void HandleActions() // �׼� �� �޴� �Լ�
    {
        moveInput = moveAction.ReadValue<Vector2>();  // moveAction �� �ʱ�ȭ

        jumpInput = jumpAction.WasPressedThisFrame(); // jumpAction �� �ʱ�ȭ 

        anyKeyInput = anyKeyAction.WasPressedThisFrame();   
    }

    // ����Ʈ ���� �����ϴ� �Լ�
    public void SwitchToActionMap(string actionMapName)
    {
        if (playerInput != null)
        {
            // ���� ���� ����
            playerInput.SwitchCurrentActionMap(actionMapName);
        }
    }

    void OnDestroy()
    {
        GameLogicManager.Instance.DeregisterUpdatableObject(this);
    }
}
