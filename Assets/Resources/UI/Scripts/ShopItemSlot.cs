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

    private string resourceKey;

    /*public void Init(string itemName, int price, Sprite iconSprite)
    {
        this.itemName = itemName;
        this.price = price;
        this.resourceKey = null;

        textName.text = itemName;
        textPrice.text = price.ToString();
        icon.sprite = iconSprite;
    }
    */

    public void Init(ShopItemEntry entry)
    {
        if (entry == null) return;

        this.itemName = entry.itemName;
        this.price = entry.price;
        this.resourceKey = entry.resourceKey;

        textName.text = entry.itemName;
        textPrice.text = entry.price.ToString();
        icon.sprite = entry.icon;
    }

    public void SetResourceKey(string key) => resourceKey = key;

    public void OnClick()
    {
        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        Debug.Log($"[Slot.Click] {gameObject.name} key={resourceKey}");
        if (!string.IsNullOrEmpty(resourceKey))
            PurchasePopup.Instance.Open(itemName, price, resourceKey);
        else
            PurchasePopup.Instance.Open(itemName, price);
    }
}
