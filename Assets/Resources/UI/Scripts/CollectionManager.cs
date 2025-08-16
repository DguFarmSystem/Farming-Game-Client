using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance;

    [Header("UI 참조")]
    public CanvasGroup CollectionPanel;      // 전체 패널
    public RectTransform bookRect;           // 책 UI
    public float slideDuration = 0.5f;       // 슬라이드 시간
    public Image bookImage;                  // 배경 이미지
    public Sprite[] gradeBooks;              // 0~3 등급별 배경

    private Vector2 hiddenPos;

    private void Awake()
    {
        CollectionPanel.alpha = 0f;
        CollectionPanel.blocksRaycasts = false;
        hiddenPos = new Vector2(0, -Screen.height);
        bookRect.anchoredPosition = hiddenPos;

        gameObject.SetActive(false);
    }

    public void Open()
    {
        CollectionPanel.transform.SetAsLastSibling();
        gameObject.SetActive(true);

        CollectionPanel.DOFade(1, 0.3f);
        CollectionPanel.blocksRaycasts = true;
        bookRect.DOAnchorPosY(0, slideDuration).SetEase(Ease.OutBack);

        SetGrade(0);

        CollectionSlotManager.Instance.ShowSlots();
    }

    public void Close()
    {
        CollectionPanel.blocksRaycasts = false;
        bookRect.DOAnchorPos(hiddenPos, slideDuration).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                CollectionPanel.DOFade(0, 0.3f).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            });
    }

    public void SetGrade(int index)
    {
        if (index >= 0 && index < gradeBooks.Length)
            bookImage.sprite = gradeBooks[index];
    }
}
