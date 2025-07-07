using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public int[] item_Count = { }; // 1 - 1시간, 2 - 2시간, 3 - 4시간

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    public bool Use(int item_Id)
    {
        if (item_Count[item_Id] <= 0) return false;

        item_Count[item_Id]--;
        return true;
    }

    public int GetCount(int item_Id)
    {
        return item_Count[item_Id];
    }
}
