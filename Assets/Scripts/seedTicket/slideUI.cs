using UnityEngine;
using DG.Tweening; // DOTween 네임스페이스

public class SlideInUI : MonoBehaviour
{
    public RectTransform targetUI; // 움직일 UI
    public float moveDistance = 300f; // 얼마나 아래에서 시작할지
    public float duration = 0.5f;     // 애니메이션 시간

    private void OnEnable()
    {
        // 현재 anchoredPosition 저장
        Vector2 endPos = targetUI.anchoredPosition;

        // 시작 위치를 화면 아래로 내리기
        targetUI.anchoredPosition = endPos - new Vector2(0, moveDistance);

        // 부드럽게 위로 이동
        targetUI.DOAnchorPos(endPos, duration)
            .SetEase(Ease.OutCubic); // Ease 타입은 취향대로
    }
}
