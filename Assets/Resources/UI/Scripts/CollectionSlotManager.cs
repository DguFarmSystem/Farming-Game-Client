using System.Linq;
using UnityEngine;

public class CollectionSlotManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject slotPrefab;   // FlowerSlotUI 프리팹
    [SerializeField] private Transform contentParent; // GridLayoutGroup가 붙은 Content

    public void ShowSlots(int gradeIndex)
    {
        var db = FlowerDataManager.Instance;
        if (db == null || db.flowerData == null || db.flowerData.flowerList == null)
        {
            Debug.LogWarning("[CollectionSlotManager] FlowerData가 비어있습니다.");
            return;
        }

        var list = db.flowerData.flowerList
            .Where(f => f != null && RarityToIndex(f.rarity) == gradeIndex)
            .OrderBy(f => f.flowerName, System.StringComparer.Ordinal);

        // 기존 슬롯 삭제
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        // 생성/채우기
        int index = 0;
        foreach (var f in list)
        {
            var go = Instantiate(slotPrefab, contentParent);
            var slot = go.GetComponent<FlowerSlotUI>();
            if (slot == null) { Debug.LogError("FlowerSlotUI 컴포넌트 없음"); continue; }

            slot.Init();

            // isRegistered 에 따라 매니저가 원본/실루엣 반환
            Sprite sprite = db.GetFlowerSprite(f.flowerName);
            bool collected = f.isRegistered;

            // silhouetteSprite가 비었을 수도 있으니 null 대비
            if (sprite == null) sprite = f.originalSprite;

            slot.SetSprite(sprite, collected, f.flowerName);
            index++;
        }
    }

    int RarityToIndex(FlowerRarity r) =>
        r == FlowerRarity.Normal ? 0 :
        r == FlowerRarity.Rare ? 1 :
        r == FlowerRarity.Epic ? 2 : 3;
}
