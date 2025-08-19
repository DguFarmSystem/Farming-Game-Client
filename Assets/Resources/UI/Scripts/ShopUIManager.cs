using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class ShopUIManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private ShopDatabase database;

    [Header("UI")]
    [SerializeField] private GameObject shopItemSlotPrefab;
    [SerializeField] private Transform tileContentParent;
    [SerializeField] private Transform objectContentParent;

    public RectTransform shopPanel;
    public GameObject tileTabPanel;
    public GameObject objectTabPanel;

    private Vector2 shownPos;
    private Vector2 hiddenPos;
    private float slideDuration = 0.5f;

    private readonly List<ShopItemEntry> tileItems = new();
    private readonly List<ShopItemEntry> objectItems = new();

    private void Awake()
    {
        shownPos = shopPanel.anchoredPosition;
        hiddenPos = shownPos + new Vector2(0, -Screen.height);

        shopPanel.anchoredPosition = hiddenPos;
        shopPanel.gameObject.SetActive(false);

        LoadShopItems();
    }

    public void OpenShopPanel()
    {
        shopPanel.gameObject.SetActive(true);
        shopPanel.anchoredPosition = hiddenPos;

        shopPanel.DOAnchorPos(shownPos, slideDuration).SetEase(Ease.OutCubic);

        OpenTileTab();
    }

    public void CloseShopPanel()
    {
        shopPanel.DOAnchorPos(hiddenPos, slideDuration).SetEase(Ease.InCubic)
        .OnComplete(() =>
        {
            shopPanel.gameObject.SetActive(false);
        });
    }

    public void OpenTileTab()
    {
        tileTabPanel.SetActive(true);
        objectTabPanel.SetActive(false);
        PopulateShopItems(tileItems, tileContentParent);
    }

    public void OpenObjectTab()
    {
        tileTabPanel.SetActive(false);
        objectTabPanel.SetActive(true);
        PopulateShopItems(objectItems, objectContentParent);
    }

    private void LoadShopItems()
    {
        tileItems.Clear();
        objectItems.Clear();

        if (database == null || database.items == null || database.items.Count == 0)
        {
            Debug.LogWarning("[Shop] ShopDatabase가 비었거나 연결되지 않았습니다.");
            return;
        }

        foreach (var e in database.items)
        {
            if (e == null) continue;
            if (e.category == ShopCategory.Tile) tileItems.Add(e);
            else if (e.category == ShopCategory.Object) objectItems.Add(e);
        }

        Debug.Log($"[Shop] 타일 {tileItems.Count} / 오브젝트 {objectItems.Count} (DB 순서 유지)");
    }

    private void PopulateShopItems(List<ShopItemEntry> items, Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);

        foreach (var item in items)
        {
            var slotGO = Instantiate(shopItemSlotPrefab, parent);

            var button = slotGO.GetComponentInChildren<Button>(true);
            ShopItemSlot comp = null;
            if (button != null) comp = button.GetComponent<ShopItemSlot>();
            if (comp == null) comp = slotGO.GetComponent<ShopItemSlot>();
            if (comp == null) comp = slotGO.GetComponentInChildren<ShopItemSlot>(true);

            if (comp == null)
            {
                Debug.LogError("ShopItemSlot 컴포넌트를 찾을 수 없습니다.");
                continue;
            }

            comp.Init(item);

            Debug.Log($"[Populate] {comp.gameObject.name} key={item.resourceKey}");
        }

        Debug.Log("[Shop] 슬롯 생성 완료");
    }
}
