using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;

public class PurchasePopup : MonoBehaviour
{
    public RectTransform popupPanel;
    public TMP_Text itemNameText;
    public TMP_Text totalPriceText;
    public TMP_InputField inputField;
    public Button plusButton;
    public Button minusButton;
    public Button confirmButton;
    public Button rejectButton;

    public static PurchasePopup Instance { get; private set; }

    public int currentCount = 1;
    public int unitPrice;
    public string itemName;
    private string resourceKey;

    public float slideDuration = 0.5f;
    private Vector2 hiddenPos;
    private Vector2 shownPos;

    [Header("구매 완료 패널")]
    [SerializeField] private GameObject purchaseCompletePanel;
    [SerializeField] private TMP_Text completeText;
    [SerializeField] private RectTransform completePanelRect;
    [SerializeField] private Button completeConfirmButton;

    public float completeSlideDuration = 0.4f;
    private Vector2 completePanelStartPos;
    private Vector2 completePanelShownPos;

    [Header("구매 실패 패널")]
    [SerializeField] private GameObject purchaseFailPanel;
    [SerializeField] private TMP_Text failText;
    [SerializeField] private RectTransform failPanelRect;
    [SerializeField] private Button failConfirmButton;

    public float failSlideDuration = 0.4f;
    private Vector2 failPanelStartPos;
    private Vector2 failPanelShownPos;

    [System.Serializable]
    public class PurchaseEvent : UnityEvent<string, string, int> { }

    [Header("구매 성공 시 알림 (resourceKey, displayName, count)")]
    public PurchaseEvent onPurchased;

    [Header("Single-Qty Item")]
    [SerializeField] private string singleQtyResourceKey = "Tilemap_Upgrade";
    private bool isSingleQtyItem = false;

    [SerializeField] private string deferDispatchKey = "Tilemap_Upgrade";
    private bool deferPurchase;
    private string pendingKey;
    private string pendingItemName;
    private int pendingCount;

    private void Awake()
    {
        Instance = this;

        // 기존 PurchasePopup
        shownPos = popupPanel.anchoredPosition;
        hiddenPos = shownPos + new Vector2(0, -Screen.height);
        popupPanel.anchoredPosition = hiddenPos;

        // 구매 완료 패널
        completePanelShownPos = completePanelRect.anchoredPosition;
        completePanelStartPos = completePanelShownPos + new Vector2(0, -Screen.height);
        completePanelRect.anchoredPosition = completePanelStartPos;
        purchaseCompletePanel.SetActive(false);

        // 구매 실패 패널
        if (failPanelRect != null)
        {
            failPanelShownPos = failPanelRect.anchoredPosition;
            failPanelStartPos = failPanelShownPos + new Vector2(0, -Screen.height);
            failPanelRect.anchoredPosition = failPanelStartPos;
        }
        if (purchaseFailPanel != null) purchaseFailPanel.SetActive(false);

        plusButton.onClick.AddListener(() => ChangeCount(1));
        minusButton.onClick.AddListener(() => ChangeCount(-1));
        confirmButton.onClick.AddListener(OnConfirm);
        rejectButton.onClick.AddListener(Close);
        inputField.onEndEdit.AddListener(OnInputChanged);
        completeConfirmButton.onClick.AddListener(HideCompletePanel);
        if (failConfirmButton != null) failConfirmButton.onClick.AddListener(HideFailPanel);
    }

    public void Open(string itemName, int pricePerItem)
    {
        deferPurchase = false;
        pendingKey = null; pendingItemName = null; pendingCount = 0;
        Open(itemName, pricePerItem, null);
    }

    public void Open(string itemName, int pricePerItem, string resourceKey)
    {
        this.itemName = itemName;
        this.unitPrice = pricePerItem;
        this.resourceKey = resourceKey;

        isSingleQtyItem = (!string.IsNullOrEmpty(resourceKey) && resourceKey == singleQtyResourceKey);

        // 초기화
        currentCount = 1;
        gameObject.SetActive(true);
        popupPanel.gameObject.SetActive(true);
        popupPanel.anchoredPosition = hiddenPos;
        popupPanel.DOAnchorPos(shownPos, slideDuration).SetEase(Ease.OutBack);

        itemNameText.text = $"{itemName}을(를)\n구매하시겠습니까?";
        SetupQtyUI(isSingleQtyItem);
        UpdateTotalPrice();
    }

    private void SetupQtyUI(bool single)
    {
        currentCount = 1;
        inputField.text = "1";

        if (plusButton) plusButton.interactable = !single;
        if (minusButton) minusButton.interactable = !single;

        if (inputField)
        {
            inputField.readOnly = single;
            inputField.interactable = !single;
        }
    }

    private void ChangeCount(int delta)
    {
        if (isSingleQtyItem) return;
        currentCount = Mathf.Clamp(currentCount + delta, 1, 999);
        inputField.text = currentCount.ToString();
        UpdateTotalPrice();
    }

    private void OnInputChanged(string val)
    {
        if (isSingleQtyItem) { inputField.text = "1"; UpdateTotalPrice(); return; }

        if (int.TryParse(val, out int num))
            currentCount = Mathf.Clamp(num, 1, 999);
        else
            currentCount = 1;

        inputField.text = currentCount.ToString();
        UpdateTotalPrice();
    }

    private void UpdateTotalPrice()
    {
        int total = unitPrice * Mathf.Max(1, currentCount);
        totalPriceText.text = $": {total}G";
    }

    private void OnConfirm()
    {
        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        int finalCount = (resourceKey == singleQtyResourceKey) ? 1 : Mathf.Clamp(currentCount, 1, 999);
        int totalCost = Mathf.Max(0, unitPrice) * finalCount;

        popupPanel.gameObject.SetActive(false);

        var cm = CurrencyManager.Instance;
        if (cm == null) { ShowPurchaseFailMessage("구매 실패: 재화 시스템을 찾을 수 없습니다."); return; }

        if (cm.SpendGold(totalCost))
        {
            bool shouldDefer = !string.IsNullOrEmpty(resourceKey) && resourceKey == deferDispatchKey;
            if (shouldDefer)
            {
                deferPurchase = true;
                pendingKey = resourceKey;
                pendingItemName = itemName;
                pendingCount = finalCount;
            }
            else
            {
                try { onPurchased?.Invoke(resourceKey, itemName, finalCount); }
                catch (System.Exception e) { Debug.LogError($"onPurchased invoke error: {e}"); }
            }

            ShowPurchaseCompleteMessage(itemName, finalCount);
        }
        else
        {
            ShowPurchaseFailMessage("구매 실패! 골드가 부족합니다.");
        }
    }

    public void Close()
    {
        GameManager.Sound.SFXPlay("SFX_ButtonCancle");
        popupPanel.DOAnchorPos(hiddenPos, slideDuration).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                popupPanel.gameObject.SetActive(false);
                gameObject.SetActive(false);
            });
    }

    private void ShowPurchaseCompleteMessage(string name, int count)
    {
        completeText.text = $"{name} {count}개를 구매하였습니다.";
        completePanelRect.anchoredPosition = completePanelShownPos;
        purchaseCompletePanel.SetActive(true);
    }

    private void HideCompletePanel()
    {
        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        completePanelRect.DOAnchorPos(completePanelStartPos, completeSlideDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                purchaseCompletePanel.SetActive(false);

            if (deferPurchase)
                {
                    var key = pendingKey;
                    var name = pendingItemName;
                    var cnt = pendingCount;

                deferPurchase = false;
                    pendingKey = null; pendingItemName = null; pendingCount = 0;

                    try { onPurchased?.Invoke(key, name, cnt); }
                    catch (System.Exception e) { Debug.LogError($"onPurchased (deferred) error: {e}"); }
                }
            });
    }


    private void ShowPurchaseFailMessage(string msg)
    {
        if (failText != null) failText.text = msg;
        if (failPanelRect != null) failPanelRect.anchoredPosition = failPanelShownPos;
        if (purchaseFailPanel != null) purchaseFailPanel.SetActive(true);
        else Debug.LogWarning("구매 실패 패널이 설정되지 않았습니다.");
    }

    private void HideFailPanel()
    {
        if (failPanelRect == null || purchaseFailPanel == null)
        {
            if (purchaseFailPanel != null) purchaseFailPanel.SetActive(false);
            return;
        }

        failPanelRect.DOAnchorPos(failPanelStartPos, failSlideDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() => purchaseFailPanel.SetActive(false));
    }
}
