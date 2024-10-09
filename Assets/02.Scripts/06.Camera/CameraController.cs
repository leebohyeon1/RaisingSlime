using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour, IUpdateable
{
    [SerializeField] private Transform player; // 플레이어의 Transform (추적 대상)
    private CinemachineVirtualCamera virtualCamera; // Cinemachine 가상 카메라

    // 기본 카메라 거리와 플레이어 스케일에 따른 비율
    public float baseCameraDistance = 10f; // 기본 카메라 거리
    public float scaleMultiplier = 2f; // 플레이어 스케일에 따라 카메라 거리 조정 비율

    private CinemachineTransposer transposer; // 카메라의 Follow Offset을 조정하는 데 사용할 Transposer

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
        
        // 가상 카메라에서 CinemachineTransposer 가져오기
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

        // 기본 카메라 거리를 설정 (초기화)
        //UpdateCameraDistance();

        GameLogicManager.Instance.RegisterUpdatableObject(this);
    }

    public void OnUpdate(float dt)
    {
        UpdateCameraDistance();
    }

    void UpdateCameraDistance()
    {
        // 플레이어의 크기(스케일)를 가져옴
        float playerScale = player.localScale.magnitude; // 3차원 벡터에서 크기를 계산

        // 플레이어의 크기에 따라 카메라 거리를 조정
        float newCameraDistance = baseCameraDistance + (playerScale * scaleMultiplier);

        // CinemachineTransposer의 Follow Offset에서 Z축(거리) 조정
        transposer.m_FollowOffset.z = -newCameraDistance; // 음수 값으로 멀어지도록 설정
        transposer.m_FollowOffset.y = newCameraDistance;
    }

    void OnDestroy()
    {
        GameLogicManager.Instance.DeregisterUpdatableObject(this);
    }
}
