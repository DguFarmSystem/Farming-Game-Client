using UnityEngine;
using UnityEngine.UI;

public class SellQuickMenu : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform root;
    [SerializeField] private Button sellButton;       // 판매 버튼
    [SerializeField] private Button cantSellButton;   // 판매 불가
    [SerializeField] private CanvasGroup cg;

    private ObjectDatabase db;
    private int index, price;
    private SellPopup popupPrefab;
    private Canvas _canvas;

    void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();

        if (sellButton)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() =>
            {
                Instantiate(popupPrefab, _canvas.transform).Open(db, index, price);
                Hide();
            });
        }
        if (cantSellButton)
        {
            cantSellButton.onClick.RemoveAllListeners();
            cantSellButton.onClick.AddListener(Hide);
        }
    }

    public void Show(ObjectDatabase db, int index, SellPopup popup, int price,
                     Vector2 screenPos, bool canSell)
    {
        this.db = db; this.index = index; this.popupPrefab = popup; this.price = price;

        if (sellButton) sellButton.gameObject.SetActive(canSell);
        if (cantSellButton) cantSellButton.gameObject.SetActive(!canSell);

        if (_canvas && root)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                screenPos, _canvas.worldCamera, out var lp);
            root.anchoredPosition = lp;
        }

        gameObject.SetActive(true);
        if (cg) { cg.alpha = 1f; cg.blocksRaycasts = true; }
    }

    public void HideImmediate()
    {
        if (cg) { cg.alpha = 0f; cg.blocksRaycasts = false; }
        gameObject.SetActive(false);
    }

    public void Hide() => HideImmediate();
}
