using System.Collections.Generic;
using UnityEngine;

public enum ShopCategory { Tile, Object }

[System.Serializable]
public class ShopItemEntry
{
    public string itemName;       // 화면표시 이름
    public string resourceKey;    // 리소스 키
    public int price;             // 골드
    public ShopCategory category; // 분류
    public Sprite icon;           // 아이콘
}

[CreateAssetMenu(fileName = "ShopDatabase", menuName = "Shop/Database")]
public class ShopDatabase : ScriptableObject
{
    public List<ShopItemEntry> items = new();
}
