using UnityEngine;
using UnityEngine.EventSystems;

public class RightClickSell : MonoBehaviour, IPointerClickHandler
{
    int index, price; ObjectDatabase db; SellPopup popupPrefab; SellQuickMenu menu;

    public void Init(int i, ObjectDatabase d, SellPopup popup, int unitPrice, SellQuickMenu quickMenu = null)
    {
        index = i; db = d; popupPrefab = popup; price = unitPrice; menu = quickMenu;
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Right) return;

        if (price <= 0) return;

        if (menu != null)
        {
            menu.Show(db, index, popupPrefab, price, e.position);
        }
        else
        {
            var canvas = GetComponentInParent<Canvas>()?.transform ?? transform;
            if (popupPrefab != null)
                Instantiate(popupPrefab, canvas).Open(db, index, price);
        }
    }
}