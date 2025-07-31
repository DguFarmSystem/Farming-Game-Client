using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public PlantUI plantUI;
    public HarvestUI harvestUI;
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

    public void HideAll()
    {
        plantUI?.Hide();
        harvestUI?.Hide();
    }

    public void ShowShopUI()
    {
        //유아이 켜주기
        shopUI.SetActive(!shopUI.activeSelf);
    }
}
