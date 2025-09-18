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

    public bool HasValidContent()
    {
        if (objectDatabase == null) return false;
        if (objectSelectButtonPrefab == null) return false;
        if (parents == null || parents.Length == 0) return false;

        foreach (var p in parents)
        {
            if (p == null) return false;
            if (p.gameObject == null) return false;
        }
        return true;
    }

    public void Open()
    {
        Debug.Log("BagManager.Open 호출됨");

        // 열기 사운드
        try { GameManager.Sound.SFXPlay("SFX_ButtonClick"); } catch { }

        gameObject.SetActive(true);
        windowRect.DOKill();
        panelGroup.DOKill();

        // 서버에서 인벤토리 갱신 후 재구성
        FetchInventoryAndRebuild();

        panelGroup.blocksRaycasts = true;
        windowRect.DOAnchorPosY(0, slideDuration).SetEase(Ease.OutBack);
        panelGroup.alpha = 1f;
    }

    public void Close()
    {
        // 닫기 사운드
        try { GameManager.Sound.SFXPlay("SFX_ButtonCancle"); } catch { }

        windowRect.DOKill();
        panelGroup.DOKill();

        panelGroup.blocksRaycasts = false;
        windowRect.DOAnchorPos(new Vector2(0, -Screen.height), slideDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => gameObject.SetActive(false));
    }

    public void Rebuild()
    {
        if (!isActiveAndEnabled)
        {
            Debug.LogWarning("[Bag] Rebuild skipped: BagManager disabled");
            return;
        }
        if (!HasValidContent())
        {
            Debug.LogWarning("[Bag] Rebuild skipped: parents or content missing/destroyed");
            return;
        }

        BuildBagSlots();
    }

    public void FetchInventoryAndRebuild()
    {
        if (APIManager.Instance == null || objectDatabase == null)
        {
            Debug.LogError("[Bag] APIManager 또는 ObjectDatabase 누락");
            BuildBagSlots();
            return;
        }

        APIManager.Instance.Get("/api/inventory",
            ok => { objectDatabase.ApplyInventoryJson(ok, true); BuildBagSlots(); },
            err => { Debug.LogError("[Bag] 인벤토리 로드 실패: " + err); BuildBagSlots(); }
        );
    }

    private void BuildBagSlots()
    {
        foreach (var parent in parents)
        {
            if (parent == null)
            {
                Debug.LogWarning("[Bag] Skip a destroyed parent container.");
                continue;
            }

            for (int i = parent.childCount - 1; i >= 0; --i)
            {
                var child = parent.GetChild(i);
                if (child != null) Destroy(child.gameObject);
            }
        }

        int total = objectDatabase.GetItemCount();

        for (int i = 0; i < total; i++)
        {
            int count = objectDatabase.GetCountFromIndex(i);
            if (count <= 0) continue;

            string id = objectDatabase.GetID(i);
            string name = objectDatabase.GetName(i);
            Sprite sprite = objectDatabase.GetSprite(i);
            var type = objectDatabase.GetType(i);

            int idx = (int)type;
            if (idx < 0 || idx >= parents.Length || parents[idx] == null)
            {
                Debug.LogWarning($"[Bag] Invalid parent for type={type} (idx={idx}). Skip {id}");
                continue;
            }

            Transform parent = parents[idx];

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

            //bool sellable = objectDatabase.IsSellable(i);

            int sellPrice = objectDatabase.GetSellPrice(i);

            // 우클릭 판매 연결
            var rcSell = slotObj.GetComponent<RightClickSell>();
            if (!rcSell) rcSell = slotObj.AddComponent<RightClickSell>();
            rcSell.Init(i, objectDatabase, sellPopupPrefab, objectDatabase.GetSellPrice(i), quickMenu);
        }
    }
}
