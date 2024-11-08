using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour, IUpdateable
{
    [SerializeField] private Transform player; // �÷��̾��� Transform (���� ���)
    private CinemachineVirtualCamera virtualCamera; // Cinemachine ���� ī�޶�

    // �⺻ ī�޶� �Ÿ��� �÷��̾� �����Ͽ� ���� ����
    public float baseCameraDistance = 25f; // �⺻ ī�޶� �Ÿ�
    public float scaleMultiplier = 2f; // �÷��̾� �����Ͽ� ���� ī�޶� �Ÿ� ���� ����

    private CinemachineTransposer transposer; // ī�޶��� Follow Offset�� �����ϴ� �� ����� Transposer

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    private void Start()
    {
      
        
        // ���� ī�޶󿡼� CinemachineTransposer ��������
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

        // �⺻ ī�޶� �Ÿ��� ���� (�ʱ�ȭ)
        //UpdateCameraDistance();

        Invoke("SetPlayer", 0.1f);

        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    private void SetPlayer()
    {
        if (player == null)
        {
            player = SpawnManager.Instance.GetPlayerTrans();

            if (player != null)
            {
                virtualCamera.Follow = player.transform;

                virtualCamera.LookAt = player.transform;
            }
        }
    }
    public void OnUpdate(float dt)
    {
        if (player == null)
        {
            return;
        }

        UpdateCameraDistance();
    }

    void UpdateCameraDistance()
    {
        // �÷��̾��� ũ��(������)�� ������
        float playerScale = player.localScale.magnitude; // 3���� ���Ϳ��� ũ�⸦ ���

        // �÷��̾��� ũ�⿡ ���� ī�޶� �Ÿ��� ����
        float newCameraDistance = baseCameraDistance + (playerScale * scaleMultiplier);

        // CinemachineTransposer�� Follow Offset���� Z��(�Ÿ�) ����
        transposer.m_FollowOffset.z = - (newCameraDistance * 4) /5; // ���� ������ �־������� ����
        transposer.m_FollowOffset.x = (newCameraDistance * 4) / 5;
        transposer.m_FollowOffset.y = newCameraDistance;
    }

    void OnDestroy()
    {
        GameLogicManager.Instance.DeregisterUpdatableObject(this);
    }
}
