using UnityEngine;
using System.Collections.Generic;

public class BagSlotManager : MonoBehaviour
{
    public static BagSlotManager Instance;

    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private int testSlotCount = 20;

    private List<GameObject> slots = new();

    void Awake()
    {
        Instance = this;
        Debug.Log("BagSlotManager 초기화됨");
    }

    public void ShowSlots()
    {
        Debug.Log("ShowSlots 호출됨");

        if (slots.Count == 0)
            GenerateSlots(testSlotCount);
    }

    private void GenerateSlots(int count)
    {
        Debug.Log($"슬롯 {count}개 생성 시도");

        for (int i = 0; i < count; i++)
        {
            GameObject slot = Instantiate(slotPrefab, contentParent);
            slot.name = $"Slot_{i}";
            slots.Add(slot);
        }

        Debug.Log("슬롯 생성 완료");
    }


}
