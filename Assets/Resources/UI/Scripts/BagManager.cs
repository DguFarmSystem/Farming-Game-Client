using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class BagManager : MonoBehaviour
{
    public static BagManager Instance;

    [Header("Bag UI")]
    public CanvasGroup panelGroup;
    public RectTransform windowRect;
    public float slideDuration = 0.5f;
    [SerializeField] private Button closeButton;

    [Header("Database")]
    [SerializeField] private ObjectDatabase objectDatabase;
    [SerializeField] private GameObject objectSelectButtonPrefab;
    [SerializeField] private Transform[] parents;

    [Header("Sell UI")]
    [SerializeField] private SellPopup sellPopupPrefab;

    [SerializeField] private SellQuickMenu quickMenuPrefab;
    SellQuickMenu quickMenu;

    private Vector2 hiddenPos;

    [SerializeField] private int defaultSellPrice = 100;

    void Awake()
    {
        Debug.Log($"QuickMenu made? {quickMenu != null}");

        Instance = this;
        panelGroup.alpha = 0f;
        panelGroup.blocksRaycasts = false;
        hiddenPos = new Vector2(0, -Screen.height);
        windowRect.anchoredPosition = hiddenPos;
        gameObject.SetActive(false);

        if (closeButton != null) closeButton.onClick.AddListener(Close);

        if (quickMenuPrefab != null && quickMenu == null)
        {
            var canvas = GetComponentInParent<Canvas>()?.transform ?? transform;
            quickMenu = Instantiate(quickMenuPrefab, canvas);
            quickMenu.HideImmediate();
        }
    }

    public void Open()
    {
        Debug.Log("BagManager.Open 호출됨");

        gameObject.SetActive(true);
        windowRect.DOKill();
        panelGroup.DOKill();

        Rebuild();

        panelGroup.blocksRaycasts = true;
        windowRect.DOAnchorPosY(0, slideDuration).SetEase(Ease.OutBack);

        panelGroup.alpha = 1f;
    }

    public void Close()
    {
        windowRect.DOKill();
        panelGroup.DOKill();

        panelGroup.blocksRaycasts = false;
        windowRect.DOAnchorPos(new Vector2(0, -Screen.height), slideDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => gameObject.SetActive(false));
    }

    public void Rebuild()
    {
        BuildBagSlots();
    }

    private void BuildBagSlots()
    {
        // 기존 슬롯 제거
        foreach (Transform parent in parents)
        {
            foreach (Transform child in parent)
            {
                Destroy(child.gameObject);
            }
        }

        int totalItems = objectDatabase.GetItemCount();

        for (int i = 0; i < totalItems; i++)
        {
            string name = objectDatabase.GetName(i);
            Sprite sprite = objectDatabase.GetSprite(i);
            int count = objectDatabase.GetCountFromIndex(i);

            if (count <= 0) continue;

            var slotObj = Instantiate(objectSelectButtonPrefab);
            var objSelectButton = slotObj.GetComponent<ObjectSelectButton>();
            objSelectButton.Init(name, sprite, count);

            PlaceType type = objectDatabase.GetType(i);
            switch (type)
            {
                case PlaceType.Tile:
                    slotObj.transform.SetParent(parents[0], false);
                    break;
                case PlaceType.Object:
                    slotObj.transform.SetParent(parents[1], false);
                    break;
                case PlaceType.Plant:
                    slotObj.transform.SetParent(parents[2], false);
                    break;
            }

            // 판매 기능 
            var rcSell = slotObj.AddComponent<RightClickSell>();
            rcSell.Init(i, objectDatabase, sellPopupPrefab, defaultSellPrice, quickMenu);

        }
    }
}