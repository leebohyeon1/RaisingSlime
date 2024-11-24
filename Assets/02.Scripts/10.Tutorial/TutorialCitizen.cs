using DG.Tweening;
using UnityEngine;

public class TutorialCitizen : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpDuration = 0.25f; // 점프 지속 시간
    [SerializeField] private float jumpHeight = 0.6f; // 점프 높이

    private bool isJumping = false; // 현재 점프 상태
    private Vector3 initialPosition; // 초기 위치 저장

    private void Start()
    {
        initialPosition = transform.position; // 초기 위치 저장
        StartJumpLoop(); // 점프 루프 시작
    }

    private void StartJumpLoop()
    {
        if (isJumping) return;

        isJumping = true;

        // DOTween 체인 구성
        transform.DOMoveY(initialPosition.y + jumpHeight, jumpDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOMoveY(initialPosition.y, jumpDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        isJumping = false;
                        StartJumpLoop(); // 루프 반복
                    });
            });
    }
}
