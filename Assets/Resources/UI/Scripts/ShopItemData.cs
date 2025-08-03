using UnityEngine;

public enum ShopCategory { Tile, Object }

[CreateAssetMenu(fileName = "ShopItemData", menuName = "Shop/Item")]
public class ShopItemData : ScriptableObject
{
    public string itemName;       // 화면에 표시될 이름
    public string resourceKey;    // 리소스 이름
    public int price;             // 골드 가격
    public ShopCategory category; // 타일 / 오브젝트 구분
    public Sprite icon;           // UI에 표시될 아이콘
}
