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

        var canvas = GetComponentInParent<Canvas>()?.transform ?? transform;
        if (popupPrefab == null) return;

        // 가격이 0 이하라면 "판매 불가" 팝업 띄우기
        if (price <= 0)
        {
            Instantiate(popupPrefab, canvas).Open(db, index, price);
            return;
        }

        // 판매 가능한 경우
        if (menu != null)
        {
            menu.Show(db, index, popupPrefab, price, e.position);
        }
        else
        {
            Instantiate(popupPrefab, canvas).Open(db, index, price);
        }
    }
}