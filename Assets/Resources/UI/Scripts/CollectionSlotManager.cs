using UnityEngine;
using System.Collections.Generic;

public class CollectionSlotManager : MonoBehaviour
{
    public static CollectionSlotManager Instance;

    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private int testSlotCount = 30;

    private List<FlowerSlotUI> flowerSlots = new();

    private void Awake()
    {
        Instance = this;
        Debug.Log("CollectionSlotManager 초기화됨");
    }

    public void ShowSlots()
    {
        if (flowerSlots.Count == 0)
            GenerateSlots(testSlotCount);
    }

    private void GenerateSlots(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(slotPrefab, contentParent);
            go.name = $"FlowerSlot_{i}";

            FlowerSlotUI slot = go.GetComponent<FlowerSlotUI>();
            if (slot != null)
            {
                slot.Init(i);
                flowerSlots.Add(slot);
            }
            else
            {
                Debug.LogError("FlowerSlotUI 컴포넌트 없음!");
            }
        }
    }

    // 추후 수집 처리도 여기서 처리
    public void RegisterCollectedFlower(int index, Sprite flowerSprite, string flowerName)
    {
        if (index >= 0 && index < flowerSlots.Count)
        {
            flowerSlots[index].SetCollected(flowerSprite, flowerName);
        }
    }

}
