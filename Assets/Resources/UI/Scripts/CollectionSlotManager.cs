using System.Linq;
using UnityEngine;

public class CollectionSlotManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private FlowerDataSO dex; 

    [Header("UI")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform contentParent;

    public void ShowSlots(int gradeIndex)
    {
        // 1) 도감 데이터 가져오기
        var db = FlowerDataManager.Instance;
        var list = db.flowerData.flowerList
            .Where(f => f != null && RarityToIndex(f.rarity) == gradeIndex)
            .OrderBy(f => f.flowerName, System.StringComparer.Ordinal);

        // 2) 기존 슬롯 싹 정리
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        // 3) 새 슬롯 생성 & 채우기
        int index = 0;
        foreach (var f in list)
        {
            var go = Instantiate(slotPrefab, contentParent);
            var slot = go.GetComponent<FlowerSlotUI>();
            slot.Init(index);

            bool collected = f.isRegistered;
            Sprite sprite = db.GetFlowerSprite(f.flowerName);

            slot.SetSprite(sprite, collected, f.flowerName);

            index++;
        }
    }

    int RarityToIndex(FlowerRarity r)
        => r == FlowerRarity.Normal ? 0 :
           r == FlowerRarity.Rare ? 1 :
           r == FlowerRarity.Epic ? 2 : 3;
}
