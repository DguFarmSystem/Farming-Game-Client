using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;

public class PurchasePopup : MonoBehaviour
{
    [Header("메인 팝업")]
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
    private long resourceKeyId;

    [Header("애니메이션")]
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
    public class PurchaseEvent : UnityEvent<long, string, int> { }

    [Header("구매 성공 시 알림 (resourceKeyId, displayName, count)")]
    public PurchaseEvent onPurchased;

    [Header("땅 확장권")]
    [SerializeField] private long singleQtyResourceKeyId = 400050;
    [SerializeField] private long deferDispatchKeyId = 400050;

    private bool isSingleQtyItem = false;

    private bool deferPurchase;
    private long pendingKeyId;
    private string pendingItemName;
    private int pendingCount;

    // 해상도 깨지는 거 해결용 
    Canvas rootCanvas;
    RectTransform rootCanvasRT;

    private bool shownPosCaptured = false;
    private Vector2 designShownPos;

    private void Awake()
    {
        Instance = this;

        rootCanvas = GetComponentInParent<Canvas>();
        rootCanvasRT = rootCanvas ? rootCanvas.transform as RectTransform : null;

        if (popupPanel && !shownPosCaptured)
        {
            designShownPos = popupPanel.anchoredPosition;
            shownPos = designShownPos;
            shownPosCaptured = true;
        }

        // 좌표 초기화
        RecomputePositions();

        // 처음엔 비활성화해서 화면 아래에 있어도 보이지 않게
        if (popupPanel) popupPanel.gameObject.SetActive(false);

        // 완료/실패 패널 비활성화 및 시작 좌표 세팅
        if (purchaseCompletePanel) purchaseCompletePanel.SetActive(false);
        if (purchaseFailPanel) purchaseFailPanel.SetActive(false);

        /*shownPos = popupPanel.anchoredPosition;
        hiddenPos = shownPos + new Vector2(0, -Screen.height);
        popupPanel.anchoredPosition = hiddenPos;

        completePanelShownPos = completePanelRect.anchoredPosition;
        completePanelStartPos = completePanelShownPos + new Vector2(0, -Screen.height);
        completePanelRect.anchoredPosition = completePanelStartPos;
        purchaseCompletePanel.SetActive(false);

        if (failPanelRect != null)
        {
            failPanelShownPos = failPanelRect.anchoredPosition;
            failPanelStartPos = failPanelShownPos + new Vector2(0, -Screen.height);
            failPanelRect.anchoredPosition = failPanelStartPos;
        }
        if (purchaseFailPanel != null) purchaseFailPanel.SetActive(false);*/


        // 사운드
        plusButton.onClick.AddListener(() => ChangeCount(1));
        minusButton.onClick.AddListener(() => ChangeCount(-1));
        confirmButton.onClick.AddListener(OnConfirm);
        rejectButton.onClick.AddListener(Close);
        inputField.onEndEdit.AddListener(OnInputChanged);
        completeConfirmButton.onClick.AddListener(HideCompletePanel);
        if (failConfirmButton != null) failConfirmButton.onClick.AddListener(HideFailPanel);
    }

    private void OnRectTransformDimensionsChange()
    {
        RecomputePositions();
    }

    private void RecomputePositions()
    {
        if (!popupPanel) return;

        // 표시 위치는 씬에서 배치한 값을 기준
        shownPos = designShownPos;

        // 화면 아래로 충분히 내리도록 여유값 포함
        float canvasH = rootCanvasRT ? rootCanvasRT.rect.height : Screen.height;
        float panelH = popupPanel.rect.height;
        float drop = Mathf.Max(canvasH, panelH) + 100f;

        hiddenPos = shownPos + new Vector2(0, -drop);

        // 메인 팝업이 꺼져 있으면 좌표만 숨김 위치로
        if (!popupPanel.gameObject.activeSelf)
            popupPanel.anchoredPosition = hiddenPos;

        // 완료/실패 패널 좌표도 함께 갱신
        if (completePanelRect)
        {
            completePanelShownPos = completePanelRect.anchoredPosition;
            completePanelStartPos = completePanelShownPos + new Vector2(0, -drop);
            if (purchaseCompletePanel && !purchaseCompletePanel.activeSelf)
                completePanelRect.anchoredPosition = completePanelStartPos;
        }
        if (failPanelRect)
        {
            failPanelShownPos = failPanelRect.anchoredPosition;
            failPanelStartPos = failPanelShownPos + new Vector2(0, -drop);
            if (purchaseFailPanel && !purchaseFailPanel.activeSelf)
                failPanelRect.anchoredPosition = failPanelStartPos;
        }
    }

    public void Open(string itemName, int pricePerItem)
    {
        Open(itemName, pricePerItem, 0L);
    }

    public void Open(string itemName, int pricePerItem, long resourceKeyId)
    {
        deferPurchase = false;
        pendingKeyId = 0; pendingItemName = null; pendingCount = 0;

        this.itemName = itemName;
        this.unitPrice = pricePerItem;
        this.resourceKeyId = resourceKeyId;

        isSingleQtyItem = (resourceKeyId > 0 && resourceKeyId == singleQtyResourceKeyId);

        currentCount = 1;

        popupPanel.DOKill();
        RecomputePositions();

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

        int finalCount = (resourceKeyId > 0 && resourceKeyId == singleQtyResourceKeyId)
            ? 1
            : Mathf.Clamp(currentCount, 1, 999);

        int totalCost = Mathf.Max(0, unitPrice) * finalCount;

        popupPanel.gameObject.SetActive(false);

        var cm = CurrencyManager.Instance;
        if (cm == null) { ShowPurchaseFailMessage("구매 실패: 재화 시스템을 찾을 수 없습니다."); return; }

        if (cm.SpendGold(totalCost))
        {
            bool shouldDefer = (resourceKeyId > 0 && resourceKeyId == deferDispatchKeyId);
            if (shouldDefer)
            {
                deferPurchase = true;
                pendingKeyId = resourceKeyId;
                pendingItemName = itemName;
                pendingCount = finalCount;
            }
            else
            {
                try { onPurchased?.Invoke(resourceKeyId, itemName, finalCount); }
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
                    var key = pendingKeyId;
                    var name = pendingItemName;
                    var cnt = pendingCount;

                    deferPurchase = false;
                    pendingKeyId = 0;
                    pendingItemName = null;
                    pendingCount = 0;

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
