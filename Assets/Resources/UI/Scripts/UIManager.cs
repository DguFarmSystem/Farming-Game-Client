using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public PlantUI plantUI;

    public GameObject plantPopupPrefab; //씨앗 심기 팝업 프리팹
    public GameObject seedDrawPrefab; //씨앗깡 프리팹
    public GameObject HarvestUIPrefab; // 수확 UI 프리팹
    public GameObject seedTicketPrefab; //씨앗 티켓 획득 프리팹
    public Transform popupParent; //팝업 프리팹 넣을 부모

    [SerializeField] private GameObject modalBlocker;
    int modalDepth = 0;

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
        Sprite flower_image = FlowerDataManager.Instance.GetFlowerOriginalSprite(flower_name);


        switch (FlowerDataManager.Instance.Get_Rarity(flower_name))
        {
            case "Normal":
                H_UI.Collect_UI.sprite = H_UI.normal;
                break;
            case "Rare":
                H_UI.Collect_UI.sprite = H_UI.Rare;
                break;
            case "Epic":
                H_UI.Collect_UI.sprite = H_UI.Epic;
                break;
            case "Legend":
                H_UI.Collect_UI.sprite = H_UI.Legend;
                break;
        }


        H_UI.flower_Text.text = flower_name; //꽃 이름 넘겨주기
        Debug.Log(flower_name);
        if (flower_image != null)
        {
            H_UI.flower_image.sprite = flower_image;
        }
    }

    public GameObject OpenSeedTicketPopup()
    {
        if (currentPopup != null)
            Destroy(currentPopup);

        currentPopup = Instantiate(seedTicketPrefab, popupParent);

        return currentPopup;
    }

    public void ModalPush(Transform topPanel = null)
    {
        if (!modalBlocker) return;

        modalDepth++;
        if (!modalBlocker.activeSelf) modalBlocker.SetActive(true);

        modalBlocker.transform.SetAsLastSibling();

        if (topPanel) topPanel.SetAsLastSibling();
    }

    public void ModalPop()
    {
        if (!modalBlocker) return;

        modalDepth = Mathf.Max(0, modalDepth - 1);
        if (modalDepth == 0) modalBlocker.SetActive(false);
    }

}
