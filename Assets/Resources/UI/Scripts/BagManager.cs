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

        if (!panelGroup) panelGroup = GetComponent<CanvasGroup>();
        if (!windowRect) windowRect = GetComponent<RectTransform>();

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
        foreach (var parent in parents)
            for (int i = parent.childCount - 1; i >= 0; --i)
                Destroy(parent.GetChild(i).gameObject);

        int total = objectDatabase.GetItemCount();

        for (int i = 0; i < total; i++)
        {
            int count = objectDatabase.GetCountFromIndex(i);
            if (count <= 0) continue; 

            string id = objectDatabase.GetID(i);
            string name = objectDatabase.GetName(i);
            Sprite sprite = objectDatabase.GetSprite(i);
            var type = objectDatabase.GetType(i);

            Transform parent = parents[(int)type];

            var slotObj = Instantiate(objectSelectButtonPrefab, parent, false);
            slotObj.name = $"BagSlot_{id}";

            var bagView = slotObj.GetComponent<BagItemSlotView>();
            if (bagView)
            {
                bagView.Bind(objectDatabase, i); 
            }
            else
            {
                var objSelect = slotObj.GetComponent<ObjectSelectButton>();
                if (objSelect)
                {
                    objSelect.Init(id, name, sprite, count);
                }
            }

            // 3) 우클릭 판매 연결 (퀵메뉴/바로 팝업 둘 다 지원)
            var rcSell = slotObj.GetComponent<RightClickSell>();
            if (!rcSell) rcSell = slotObj.AddComponent<RightClickSell>();
            rcSell.Init(i, objectDatabase, sellPopupPrefab, defaultSellPrice, quickMenu);
        }
    }
}