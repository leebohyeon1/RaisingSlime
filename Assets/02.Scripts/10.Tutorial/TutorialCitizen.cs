using DG.Tweening;
using UnityEngine;

public class TutorialCitizen : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpDuration = 0.25f; // ���� ���� �ð�
    [SerializeField] private float jumpHeight = 0.6f; // ���� ����

    private bool isJumping = false; // ���� ���� ����
    private Vector3 initialPosition; // �ʱ� ��ġ ����

    private void Start()
    {
        initialPosition = transform.position; // �ʱ� ��ġ ����
        StartJumpLoop(); // ���� ���� ����
    }

    private void StartJumpLoop()
    {
        if (isJumping) return;

        isJumping = true;

        // DOTween ü�� ����
        transform.DOMoveY(initialPosition.y + jumpHeight, jumpDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOMoveY(initialPosition.y, jumpDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        isJumping = false;
                        StartJumpLoop(); // ���� �ݺ�
                    });
            });
    }
}
