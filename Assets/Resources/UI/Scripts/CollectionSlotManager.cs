using System.Linq;
using UnityEngine;

public class CollectionSlotManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform contentParent;

    [Header("Lookup")]
    [SerializeField] private ObjectDatabase objectDatabase;

    public void ShowSlots(int gradeIndex)
    {
        var fdm = FlowerDataManager.Instance;
        if (fdm == null || fdm.flowerData == null || fdm.flowerData.flowerList == null)
        {
            Debug.LogWarning("[CollectionSlotManager] FlowerData가 비어있습니다.");
            return;
        }

        var list = fdm.flowerData.flowerList
            .Where(f => f != null && RarityToIndex(f.rarity) == gradeIndex)
            .OrderBy(f => GetDisplayName(f), System.StringComparer.Ordinal) 
            .ToList();

        // 기존 슬롯 삭제
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        // 생성/채우기
        foreach (var f in list)
        {
            var go = Instantiate(slotPrefab, contentParent);
            var slot = go.GetComponent<FlowerSlotUI>();
            if (!slot) { Debug.LogError("FlowerSlotUI 컴포넌트 없음"); continue; }

            slot.Init();

            // 원본/실루엣 스프라이트
            var db = fdm; // 짧게
            Sprite sprite = db.GetFlowerSprite(f.flowerName);
            if (sprite == null) sprite = f.originalSprite;

            bool collected = f.isRegistered;
            string displayName = GetDisplayName(f);

            slot.SetSprite(sprite, collected, displayName);
        }
    }

    private string GetDisplayName(FlowerData f)
    {
        if (f == null) return "";
        if (objectDatabase != null &&
            objectDatabase.TryGetIndexByStoreNo((long)f.dexId, out var idx))
        {
            var name = objectDatabase.GetName(idx);
            if (!string.IsNullOrEmpty(name)) return name; 
        }
        return f.flowerName ?? "";
    }

    int RarityToIndex(FlowerRarity r) =>
        r == FlowerRarity.Normal ? 0 :
        r == FlowerRarity.Rare ? 1 :
        r == FlowerRarity.Epic ? 2 : 3;
}
