using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; } // �̱��� ����

    public Vector2 moveInput { get; private set; } // �̵� ���� ��
    public bool jumpInput { get; private set; }    // ���� ��

    private PlayerInput playerInput; // �÷��̾� InputSystem

    private InputAction moveAction; 
    private InputAction jumpAction;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }  
    }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        SetUpInputActions();
    }

    void Update()
    {
        HandleActions();
    }

    private void SetUpInputActions() // ActionMap�� InputAction �ʱ�ȭ
    {
        moveAction = playerInput.actions["Move"];     
        jumpAction = playerInput.actions["Jump"];
    }

    private void HandleActions() // �׼� �� �޴� �Լ�
    {
        moveInput = moveAction.ReadValue<Vector2>();  // moveAction �� �ʱ�ȭ

        jumpInput = jumpAction.WasPressedThisFrame(); // jumpAction �� �ʱ�ȭ 
    }
}
