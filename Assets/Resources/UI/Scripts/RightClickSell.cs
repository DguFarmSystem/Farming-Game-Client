using UnityEngine;
using UnityEngine.EventSystems;

public class RightClickSell : MonoBehaviour, IPointerClickHandler
{
    int index, price;
    ObjectDatabase db;
    SellPopup popupPrefab;
    SellQuickMenu menu;
    bool sellable;

    public void Init(int i, ObjectDatabase d, SellPopup popup, int unitPrice,
                     SellQuickMenu quickMenu = null, GameObject _ = null)
    {
        index = i; db = d; popupPrefab = popup; price = unitPrice; menu = quickMenu;
        sellable = (db != null) && db.IsSellable(index);
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Right) return;

        if (menu != null)
        {
            menu.Show(db, index, popupPrefab, price, e.position, sellable);
        }
        else
        {
            if (sellable)
                Instantiate(popupPrefab, GetComponentInParent<Canvas>().transform)
                    .Open(db, index, price);
        }
    }
}
