using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public PlantUI plantUI;
    public HarvestUI harvestUI;
    public ItemUseUI itemUseUI;
    public GameObject shopUI; //상점 UI


    private void Awake() => Instance = this;

    public void ShowPlantUI(Ground ground)
    {
        HideAll();
        plantUI.Init(ground);
    }

    public void ShowHarvestUI(Ground ground)
    {
        HideAll();
        harvestUI.Init(ground);
    }

    public void ShowItemUseUI(Ground ground)
    {
        Debug.Log("ShowItemUseUI 호출됨");
        HideAll();
        itemUseUI.Init(ground);
    }

    public bool IsItemUseUIActiveFor(Ground ground)
    {
        return itemUseUI.IsVisibleFor(ground);
    }

    public void HideAll()
    {
        plantUI?.Hide();
        harvestUI?.Hide();
        itemUseUI?.Hide();
    }

    public void ShowShopUI()
    {
        //유아이 켜주기
        shopUI.SetActive(!shopUI.activeSelf);
    }
}
