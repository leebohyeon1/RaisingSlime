using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour, IUpdateable
{
    [SerializeField] private Transform player; // �÷��̾��� Transform (���� ���)
    private CinemachineVirtualCamera virtualCamera; // Cinemachine ���� ī�޶�

    // �⺻ ī�޶� �Ÿ��� �÷��̾� �����Ͽ� ���� ����
    public float baseCameraDistance = 10f; // �⺻ ī�޶� �Ÿ�
    public float scaleMultiplier = 2f; // �÷��̾� �����Ͽ� ���� ī�޶� �Ÿ� ���� ����

    private CinemachineTransposer transposer; // ī�޶��� Follow Offset�� �����ϴ� �� ����� Transposer

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    private void Start()
    {
        if(player == null)
        {
            player = FindFirstObjectByType<Player>().transform;
        }
        
        // ���� ī�޶󿡼� CinemachineTransposer ��������
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

        // �⺻ ī�޶� �Ÿ��� ���� (�ʱ�ȭ)
        //UpdateCameraDistance();

        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    public void OnUpdate(float dt)
    {
        UpdateCameraDistance();
    }

    void UpdateCameraDistance()
    {
        // �÷��̾��� ũ��(������)�� ������
        float playerScale = player.localScale.magnitude; // 3���� ���Ϳ��� ũ�⸦ ���

        // �÷��̾��� ũ�⿡ ���� ī�޶� �Ÿ��� ����
        float newCameraDistance = baseCameraDistance + (playerScale * scaleMultiplier);

        // CinemachineTransposer�� Follow Offset���� Z��(�Ÿ�) ����
        transposer.m_FollowOffset.z = -newCameraDistance; // ���� ������ �־������� ����
        transposer.m_FollowOffset.y = newCameraDistance;
    }

    void OnDestroy()
    {
        GameLogicManager.Instance.DeregisterUpdatableObject(this);
    }
}
