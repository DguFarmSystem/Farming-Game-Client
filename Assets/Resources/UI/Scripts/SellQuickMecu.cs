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
        if (u <= 0) return; 

        db = d; index = i; popupPrefab = p; unitPrice = u;

        var canvas = GetComponentInParent<Canvas>();
        if (!canvas || !panel) { Debug.LogWarning("[SellQuickMenu] canvas/panel null"); return; }


        var parentRT = panel.parent as RectTransform;
        var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, screenPos, cam, out var local))
        {
            if (blocker)
            {
                blocker.SetActive(true);
                blocker.transform.SetAsFirstSibling();
            }

            panel.gameObject.SetActive(true);
            panel.SetAsLastSibling();
            panel.anchoredPosition = local;
        }
    }

    public void HideImmediate()
    {
        if (blocker) blocker.SetActive(false);
        if (panel) panel.gameObject.SetActive(false);
    }

    void OnClickSell()
    {
        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        HideImmediate();

        var canvasTf = GetComponentInParent<Canvas>()?.transform ?? transform;
        if (popupPrefab != null)
            Instantiate(popupPrefab, canvasTf).Open(db, index, unitPrice);
        else
            Debug.LogWarning("[SellQuickMenu] popupPrefab is null");
    }
}
