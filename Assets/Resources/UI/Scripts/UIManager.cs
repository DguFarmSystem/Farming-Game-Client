using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public PlantUI plantUI;
    public GameObject shopUI; //상점 UI


    public GameObject plantPopupPrefab; //씨앗 심기 팝업 프리팹
    public GameObject seedDrawPrefab; //씨앗깡 프리팹
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
}
