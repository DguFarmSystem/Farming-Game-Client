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

        if (failConfirmButton != null)
            failConfirmButton.onClick.AddListener(HideFailPanel);
    }

    public void Open(string itemName, int pricePerItem)
    {
        Open(itemName, pricePerItem, null);
    }

    public void Open(string itemName, int pricePerItem, string resourceKey)
    {
        Debug.Log($"Open called: {itemName} / {pricePerItem}");

        this.itemName = itemName;
        this.unitPrice = pricePerItem;
        this.resourceKey = resourceKey;
        currentCount = 1;

        // 초기화 안전하게
        gameObject.SetActive(true);
        popupPanel.gameObject.SetActive(true);

        popupPanel.anchoredPosition = hiddenPos;
        popupPanel.DOAnchorPos(shownPos, slideDuration).SetEase(Ease.OutBack);

        this.itemName = itemName;
        this.unitPrice = pricePerItem;
        currentCount = 1;

        itemNameText.text = $"{itemName}을(를)\n구매하시겠습니까?";
        inputField.text = currentCount.ToString();
        UpdateTotalPrice();
    }

    private void ChangeCount(int delta)
    {
        currentCount = Mathf.Clamp(currentCount + delta, 1, 999);
        inputField.text = currentCount.ToString();
        UpdateTotalPrice();
    }

    private void OnInputChanged(string val)
    {
        if (int.TryParse(val, out int num))
        {
            currentCount = Mathf.Clamp(num, 1, 999);
        }
        else
        {
            currentCount = 1;
        }
        inputField.text = currentCount.ToString();
        UpdateTotalPrice();
    }

    private void UpdateTotalPrice()
    {
        int total = unitPrice * currentCount;
        totalPriceText.text = $": {total}G";
    }

    private void OnConfirm()
    {
        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        int totalCost = Mathf.Max(0, unitPrice) * currentCount;
        popupPanel.gameObject.SetActive(false);

        var cm = CurrencyManager.Instance;
        if (cm == null) { ShowPurchaseFailMessage("구매 실패: 재화 시스템을 찾을 수 없습니다."); return; }

        if (cm.SpendGold(totalCost))
        {
            try { onPurchased?.Invoke(resourceKey, itemName, currentCount); }
            catch (System.Exception e) { Debug.LogError($"onPurchased invoke error: {e}"); }

            ShowPurchaseCompleteMessage(itemName, currentCount);
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

        // 위치를 먼저 세팅하고 활성화 (이 부분이 핵심!)
        completePanelRect.anchoredPosition = completePanelShownPos;
        purchaseCompletePanel.SetActive(true);
    }

    private void HideCompletePanel()
    {
        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        completePanelRect.DOAnchorPos(completePanelStartPos, completeSlideDuration)
        .SetEase(Ease.InCubic)
        .OnComplete(() => purchaseCompletePanel.SetActive(false));
    }

    private void ShowPurchaseFailMessage(string msg)
    {
        if (failText != null) failText.text = msg;

        if (failPanelRect != null)
        {
            // 위치 세팅 후 즉시 표시
            failPanelRect.anchoredPosition = failPanelShownPos;
        }

        if (purchaseFailPanel != null)
            purchaseFailPanel.SetActive(true);
        else
            Debug.LogWarning("구매 실패 패널이 설정되지 않았습니다.");
    }

    private void HideFailPanel()
    {
        if (failPanelRect == null || purchaseFailPanel == null)
        {
            // 세팅이 안됐으면 그냥 끈다
            if (purchaseFailPanel != null) purchaseFailPanel.SetActive(false);
            return;
        }

        failPanelRect.DOAnchorPos(failPanelStartPos, failSlideDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() => purchaseFailPanel.SetActive(false));
    }
}