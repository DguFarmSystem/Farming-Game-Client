using UnityEngine;
using UnityEngine.UI;

public class SellQuickMenu : MonoBehaviour
{
    [SerializeField] RectTransform panel;   
    [SerializeField] Button sellButton;     
    [SerializeField] GameObject blocker;   

    ObjectDatabase db;
    int index, unitPrice;
    SellPopup popupPrefab;                 

    void Awake()
    {
        if (blocker)
        {
            var btn = blocker.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(HideImmediate); 
        }
        if (sellButton) sellButton.onClick.AddListener(OnClickSell);

        HideImmediate(); 
        gameObject.SetActive(true);
    }

    public void Show(ObjectDatabase d, int i, SellPopup p, int u, Vector2 screenPos)
    {
        db = d; index = i; popupPrefab = p; unitPrice = u; 

        var canvas = GetComponentInParent<Canvas>();
        RectTransform canvasRT = canvas.transform as RectTransform;

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out local
        );

        panel.anchoredPosition = local;

        if (blocker) blocker.SetActive(true);
        panel.gameObject.SetActive(true);
    }

    public void HideImmediate()
    {
        if (blocker) blocker.SetActive(false);
        if (panel) panel.gameObject.SetActive(false);
    }

    void OnClickSell()
    {
        HideImmediate();
        var canvas = GetComponentInParent<Canvas>()?.transform ?? transform;
        Instantiate(popupPrefab, canvas).Open(db, index, unitPrice);
    }
}
