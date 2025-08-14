using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class ShopUIManager : MonoBehaviour
{
    [SerializeField] private GameObject shopItemSlotPrefab;
    [SerializeField] private Transform tileContentParent;
    [SerializeField] private Transform objectContentParent;

    public RectTransform shopPanel;
    public GameObject tileTabPanel;
    public GameObject objectTabPanel;

    private Vector2 shownPos;
    private Vector2 hiddenPos;
    private float slideDuration = 0.5f;

    private List<ShopItemData> tileItems = new List<ShopItemData>();
    private List<ShopItemData> objectItems = new List<ShopItemData>();

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

        UIManager.Instance.ModalPush(shopPanel.transform);
        shopPanel.DOAnchorPos(shownPos, slideDuration).SetEase(Ease.OutCubic);

        OpenTileTab(); // 기본 탭 타일 탭으로 
    }

    public void CloseShopPanel()
    {
        shopPanel.DOAnchorPos(hiddenPos, slideDuration).SetEase(Ease.InCubic)
        .OnComplete(() =>
        {
            shopPanel.gameObject.SetActive(false);
            UIManager.Instance.ModalPop(); 
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

        ShopItemData[] allItems = Resources.LoadAll<ShopItemData>("UI/Image/Shop");

        foreach (var item in allItems)
        {
            if (item.category == ShopCategory.Tile)
                tileItems.Add(item);
            else if (item.category == ShopCategory.Object)
                objectItems.Add(item);
        }

        Debug.Log($"타일 아이템 수: {tileItems.Count}");
        Debug.Log($"오브젝트 아이템 수: {objectItems.Count}");
    }

    private void PopulateShopItems(List<ShopItemData> items, Transform parent)
    {
        // 기존 슬롯 제거
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        foreach (var item in items)
        {
            GameObject slot = Instantiate(shopItemSlotPrefab, parent);
            ShopItemSlot comp = slot.GetComponentInChildren<ShopItemSlot>();

            if (comp == null)
            {
                Debug.LogError("ShopItemSlot을 찾을 수 없음");
                continue;
            }

            comp.Init(item.itemName, item.price, item.icon);
        }

        Debug.Log("상점 슬롯 생성 완료");
    }
}
