using System.Collections.Generic;
using UnityEngine;

public enum BadgeType
{
    TotalRegisteredAtLeast,   // 전체 등록 ≥ threshold
    ShinyRegisteredAtLeast,   // 이로치 등록 ≥ threshold (임시: 이름에 "이로치")
    RarityAllCollected,       // 특정 등급 전종
    WhitelistAllCollected,    // 지정 이름 목록 전부 수집(과일나무 6종 등)
    UniquePlacedAtLeast       // 정원: 서로 다른 종류 배치 ≥ threshold
}

[System.Serializable]
public class BadgeEntry
{
    [Header("표시")]
    public string title;
    [TextArea] public string description;
    public Sprite icon;

    [Header("달성 조건")]
    public BadgeType type;
    public int threshold;
    public FlowerRarity rarity;
    public List<string> nameWhitelist = new();

    [Header("Reward")]
    public long rewardStoreGoodsNumber;  // 서버 object_type
    public int rewardCount = 1;
}

[CreateAssetMenu(fileName = "BadgeDatabase", menuName = "Badge/Badge Database")]
public class BadgeDatabase : ScriptableObject
{
    public List<BadgeEntry> items = new();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (items == null) items = new List<BadgeEntry>();
        // 필요하면 여기서 중복/빈 값 검사 등
    }
#endif
}
