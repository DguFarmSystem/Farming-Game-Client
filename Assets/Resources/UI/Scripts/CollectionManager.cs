using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance;

    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private Sprite[] flowerSprites;

    private List<FlowerSlotUI> flowerSlots = new();

    [Header("UI 참조")]
    public CanvasGroup CollectionPanel;       // CollectionPanel (전체 배경)
    public RectTransform bookRect;       // 도감 책 이미지 RectTransform
    public float slideDuration = 0.5f;   // 슬라이드 애니메이션 속도

    private Vector2 hiddenPos;

    public Sprite[] gradeBooks;         // 0: 노말, 1: 레어, 2: 에픽, 3: 레전드
    public Image bookImage;           // 책 배경 이미지 대상

    public void SetGrade(int index)
    {
        if (index >= 0 && index < gradeBooks.Length)
            bookImage.sprite = gradeBooks[index];
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 시작 시 도감 숨김 처리
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

        // 슬롯 생성
        if (flowerSlots.Count == 0) // 중복 생성 방지
            GenerateSlots(30);

        // 배경 페이드 + 책 올라옴
        CollectionPanel.DOFade(1, 0.3f);
        CollectionPanel.blocksRaycasts = true;
        bookRect.DOAnchorPosY(0, slideDuration).SetEase(Ease.OutBack);

        SetGrade(0);
    }

    public void Close()
    {
        CollectionPanel.blocksRaycasts = false;

        // 책 먼저 슬라이드 다운
        bookRect.DOAnchorPos(hiddenPos, slideDuration).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                // 책이 다 내려간 뒤에 페이드 아웃
                CollectionPanel.DOFade(0, 0.3f).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            });
    }

    public void GenerateSlots(int totalCount)
    {
        Debug.Log($"슬롯 생성 시작: {totalCount}개");
        for (int i = 0; i < totalCount; i++)
        {
            GameObject go = Instantiate(slotPrefab, contentParent);
            Debug.Log($"슬롯 생성됨: {go.name}");

            var slot = go.GetComponent<FlowerSlotUI>();
            if (slot != null)
                slot.Init(i);
            else
                Debug.LogError("FlowerSlotUI 컴포넌트 없음!");
        }
    }

    // 나중에 특정 슬롯을 채우는 예시
    public void RegisterCollectedFlower(int index, Sprite flowerSprite)
    {
        if (index >= 0 && index < flowerSlots.Count)
        {
            flowerSlots[index].SetCollected(flowerSprite);
        }
    }
}
