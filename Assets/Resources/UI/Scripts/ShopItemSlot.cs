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

    public void Init(string itemName, int price, Sprite iconSprite)
    {
        if (textName == null) Debug.LogError("textName is null!");
        if (textPrice == null) Debug.LogError(" textPrice is null!");
        if (icon == null) Debug.LogError("icon is null!");

        this.itemName = itemName;
        this.price = price;
        textName.text = itemName;
        textPrice.text = price.ToString();
        icon.sprite = iconSprite;
    }


    public void OnClick()
    {
        if (PurchasePopup.Instance == null)
        {
            Debug.LogError("PurchasePopup.Instance is NULL — 버튼 이벤트가 꼬였거나 초기화 안 됨");
            return;
        }

        Debug.Log("PurchasePopup 호출 성공");
        PurchasePopup.Instance.Open(itemName, price);
    }

}
