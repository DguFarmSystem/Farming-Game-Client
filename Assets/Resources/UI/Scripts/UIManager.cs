using UnityEngine;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public PlantUI plantUI;

    public GameObject plantPopupPrefab; //씨앗 심기 팝업 프리팹
    public GameObject seedDrawPrefab; //씨앗깡 프리팹
    public GameObject HarvestUIPrefab; // 수확 UI 프리팹
    public GameObject seedTicketPrefab; //씨앗 티켓 획득 프리팹
    //public Transform popupParent; //팝업 프리팹 넣을 부모

    public GameObject collectionUIPrefab;
    public GameObject TitleUIPrefab;
    public GameObject shopUIPrefab;
    public GameObject bagUIPrefab;
    
    private GameObject currentPopup; //현재 팝업

    [SerializeField] private Transform popupParent;

    private void Awake() => Instance = this;

    private Transform P()  // PopupParent 보장: 있으면 쓰고, 없으면 찾아서 만들기
    {
        if (popupParent != null && popupParent.gameObject.scene.IsValid()) return popupParent;

        //var cv = FindObjectOfType<Canvas>();                  // 씬의 Canvas 찾기
        Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
        Canvas cv = null;
        int bestOrder = int.MinValue;

        foreach (var c in canvases) {
            if (!c.isActiveAndEnabled) continue;
            if (c.gameObject.name == "FadePanel") continue; // 제외
            if (c.sortingOrder >= bestOrder) {
                bestOrder = c.sortingOrder;
                cv = c;
            }
        }
        if (cv == null) { Debug.LogError("Canvas 없음"); return null; }

        var t = cv.transform.Find("PopupParent");             // 이미 있으면 사용
        if (t == null)
        {
            var go = new GameObject("PopupParent", typeof(RectTransform)); // 없으면 생성
            var rt = (RectTransform)go.transform;
            rt.SetParent(cv.transform, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            t = rt;
        }
        t.SetAsLastSibling();                                 // 항상 최상단으로
        popupParent = t;
        return popupParent;
    }

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
            DOTween.Kill(currentPopup, complete: false);
            Destroy(currentPopup);
            currentPopup = null;
        }
    }

    public void OpenPlantPopup(FarmGround ground)
    {
        if (currentPopup != null) { DOTween.Kill(currentPopup, complete: false); Destroy(currentPopup); }

        currentPopup = Instantiate(plantPopupPrefab, popupParent);
        currentPopup.GetComponent<FarmPopup>().SetTargetTile(ground);
    }

    public bool IsPopupOpen()
    {
        return currentPopup != null && currentPopup.activeSelf;
    }

    public void OpenSeedDrawPopup()
    {
        if (currentPopup != null) { DOTween.Kill(currentPopup, complete: false); Destroy(currentPopup); }

        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        currentPopup = Instantiate(seedDrawPrefab, popupParent);
    }

    public void OpenHarvestPopup(string flower_name)
    {
        if (currentPopup != null) { DOTween.Kill(currentPopup, complete: false); Destroy(currentPopup); }

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
        if (currentPopup != null) { DOTween.Kill(currentPopup, complete: false); Destroy(currentPopup); }

        GameManager.Sound.SFXPlay("SFX_Result");
        currentPopup = Instantiate(seedTicketPrefab, popupParent);

        return currentPopup;
    }

    public void OpenCollectionUI()
    {
        var parent = P(); if (parent == null) return;
        if (currentPopup != null) { DOTween.Kill(currentPopup, complete: false); Destroy(currentPopup); }

        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        currentPopup = Instantiate(collectionUIPrefab, parent);
        currentPopup.transform.SetAsLastSibling();

        var clt = currentPopup.GetComponentInChildren<CollectionManager>();
        if (clt != null)
        {
            clt.Open();
        }
    }

    public void OpenTitleUI()
    {
        var parent = P(); if (parent == null) return;

        if (currentPopup != null)
        {
            DG.Tweening.DOTween.Kill(currentPopup, false);
            Destroy(currentPopup);
        }

        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        currentPopup = Instantiate(TitleUIPrefab, parent);
        currentPopup.transform.SetAsLastSibling();

        // 프리팹 내부에 Canvas가 있다면 정렬 올리기
        var parentCanvas = popupParent.GetComponentInParent<Canvas>();
        int baseOrder = parentCanvas ? parentCanvas.sortingOrder : 0;
        foreach (var cv in currentPopup.GetComponentsInChildren<Canvas>(true))
        {
            cv.overrideSorting = true;
            cv.sortingOrder = baseOrder + 100;
        }

        var title = currentPopup.GetComponentInChildren<BadgeManager>(true);
        if (title != null) title.Open();
    }

    public void OpenShopUI()
    {
        var parent = P(); if (parent == null) return;

        if (currentPopup != null) { DOTween.Kill(currentPopup, complete: false); Destroy(currentPopup); }

        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        currentPopup = Instantiate(shopUIPrefab, parent);

        var shop = currentPopup.GetComponentInChildren<ShopUIManager>();
        if (shop != null)
        {
            shop.OpenShopPanel();
        }
    }

    public void OpenBagUI()
    {
        var parent = P(); if (parent == null) return;

        if (currentPopup != null) { DOTween.Kill(currentPopup, complete: false); Destroy(currentPopup); }

        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        currentPopup = Instantiate(bagUIPrefab, parent);

        var bag = currentPopup.GetComponentInChildren<BagManager>(true);
        if (bag != null)
        {
            bag.Open();
        }
    }
}
