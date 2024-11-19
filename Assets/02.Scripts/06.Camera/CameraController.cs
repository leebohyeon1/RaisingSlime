using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour, IUpdateable
{
    [SerializeField] private Transform player; // �÷��̾��� Transform (���� ���)
    private CinemachineVirtualCamera virtualCamera; // Cinemachine ���� ī�޶�

    // �⺻ ī�޶� �Ÿ��� �÷��̾� �����Ͽ� ���� ����
    public float baseCameraDistance = 8f; // �⺻ ī�޶� �Ÿ�
    //public float maxCameraDistance = 15f;
    public float scaleMultiplier = 0.6f; // �÷��̾� �����Ͽ� ���� ī�޶� �Ÿ� ���� ����

    private CinemachineTransposer transposer; // ī�޶��� Follow Offset�� �����ϴ� �� ����� Transposer

    [SerializeField]
    private LayerMask transparentLayer;    

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

    public void OnUpdate(float dt)
    {
        if (player == null)
        {
            return;
        }

        UpdateCameraDistance();
    }

    private void LateUpdate()
    {
        Transparency();
    }

    private void Transparency()
    {
        if (player == null)
            return;

        Vector3 direction = (player.position - transform.position).normalized;
        float maxDistance = Vector3.Distance(transform.position, player.position); // ī�޶�� �÷��̾� ���� �Ÿ�

        // ī�޶󿡼� �÷��̾� �������� Ray�� ���, �÷��̾� ������ �������� �ʵ��� ����
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, maxDistance, transparentLayer);

        for (int i = 0; i < hits.Length; i++)
        {
            TransparentObject[] obj = hits[i].transform.GetComponentsInChildren<TransparentObject>();

            for (int j = 0; j < obj.Length; j++)
            {
                obj[j]?.BecomeTransparent();
            }
        }
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
  

    void UpdateCameraDistance()
    {
        // �÷��̾��� ũ��(������)�� ������
        float playerScale = player.localScale.magnitude; // 3���� ���Ϳ��� ũ�⸦ ���

        // �÷��̾��� ũ�⿡ ���� ī�޶� �Ÿ��� ����
        float newCameraDistance = baseCameraDistance + (playerScale * scaleMultiplier);

        // CinemachineTransposer�� Follow Offset���� Z��(�Ÿ�) ����
        transposer.m_FollowOffset.z = - newCameraDistance; // ���� ������ �־������� ����
        transposer.m_FollowOffset.x = newCameraDistance;
        transposer.m_FollowOffset.y = newCameraDistance;
    }

    void OnDestroy()
    {
        GameLogicManager.Instance.DeregisterUpdatableObject(this);
    }
}
