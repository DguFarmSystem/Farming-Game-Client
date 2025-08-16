// ShopItemSlot.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemSlot : MonoBehaviour
{
    public TMP_Text textName;
    public TMP_Text textPrice;
    public Image icon;

    public string itemName;
    public int price;

    private ShopItemData data;
    private string resourceKey;

    public void Init(string itemName, int price, Sprite iconSprite)
    {
        this.itemName = itemName;
        this.price = price;
        this.resourceKey = null;
        this.data = null;

        textName.text = itemName;
        textPrice.text = price.ToString();
        icon.sprite = iconSprite;
    }

    public void Init(ShopItemData data)
    {
        this.data = data;
        this.itemName = data.itemName;
        this.price = data.price;
        this.resourceKey = data.resourceKey;

        textName.text = data.itemName;
        textPrice.text = data.price.ToString();
        icon.sprite = data.icon;

        Debug.Log($"[Slot.Init] {gameObject.name} id={GetInstanceID()} key={resourceKey}");
    }

    public void OnClick()
    {
        Debug.Log($"[Slot.Click] {gameObject.name} id={GetInstanceID()} key={resourceKey}");
        if (!string.IsNullOrEmpty(resourceKey))
            PurchasePopup.Instance.Open(itemName, price, resourceKey);
        else
            PurchasePopup.Instance.Open(itemName, price); // fallback
    }

}
