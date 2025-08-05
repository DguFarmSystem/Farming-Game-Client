using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public PlantUI plantUI;
    public GameObject shopUI; //상점 UI


    public GameObject plantPopupPrefab; //씨앗 심기 팝업 프리팹
    public GameObject seedDrawPrefab; //씨앗깡 프리팹
    public GameObject HarvestUIPrefab; // 수확 UI 프리팹
    public Transform popupParent; //팝업 프리팹 넣을 부모



    private GameObject currentPopup; //현재 팝업

    private void Awake() => Instance = this;

    public void ShowPlantUI(FarmGround ground)
    {
        HideAll();
        plantUI.Show(ground);  // 땅 위에 버튼 띄우기
    }

    public void HideAll()
    {
        plantUI?.Hide();
        if (currentPopup != null)
        {
            Destroy(currentPopup);
            currentPopup = null;
        }
    }

    public void OpenPlantPopup(FarmGround ground)
    {
        if (currentPopup != null)
            Destroy(currentPopup);

        currentPopup = Instantiate(plantPopupPrefab, popupParent);
        currentPopup.GetComponent<FarmPopup>().SetTargetTile(ground);
    }

    public void ShowShopUI()
    {
        //유아이 켜주기
        shopUI.SetActive(!shopUI.activeSelf);
    }

    public bool IsPopupOpen()
    {
        return currentPopup != null && currentPopup.activeSelf;
    }

    public void OpenSeedDrawPopup()
    {
        if (currentPopup != null)
            Destroy(currentPopup);

        currentPopup = Instantiate(seedDrawPrefab, popupParent);
    }

    public void OpenHarvestPopup(string flower_name)
    {
        if (currentPopup != null)
            Destroy(currentPopup);

        currentPopup = Instantiate(HarvestUIPrefab, popupParent);

        HarvestUI H_UI = currentPopup.GetComponent<HarvestUI>();
        Sprite flower_image = FlowerDataManager.Instance.GetFlowerSprite(flower_name);

        H_UI.flower_Text.text = flower_name; //꽃 이름 넘겨주기
        Debug.Log(flower_name);
        if (flower_image != null)
        {
            H_UI.flower_image.sprite = flower_image;
        }
    }
}
