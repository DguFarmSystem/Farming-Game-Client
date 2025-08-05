using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

    public float slideDuration = 0.5f;
    private Vector2 hiddenPos;
    private Vector2 shownPos;

    [SerializeField] private GameObject purchaseCompletePanel;
    [SerializeField] private TMP_Text completeText;
    [SerializeField] private RectTransform completePanelRect;
    [SerializeField] private Button completeConfirmButton;

    public float completeSlideDuration = 0.4f;
    private Vector2 completePanelStartPos;
    private Vector2 completePanelShownPos;

    private void Awake()
    {
        Instance = this;

        // 기존 PurchasePopup
        shownPos = popupPanel.anchoredPosition;
        hiddenPos = shownPos + new Vector2(0, -Screen.height);
        popupPanel.anchoredPosition = hiddenPos;

        // 새로 추가된 완료 패널
        completePanelShownPos = completePanelRect.anchoredPosition;
        completePanelStartPos = completePanelShownPos + new Vector2(0, -Screen.height);
        completePanelRect.anchoredPosition = completePanelStartPos;
        purchaseCompletePanel.SetActive(false);

        plusButton.onClick.AddListener(() => ChangeCount(1));
        minusButton.onClick.AddListener(() => ChangeCount(-1));
        confirmButton.onClick.AddListener(OnConfirm);
        rejectButton.onClick.AddListener(Close);
        inputField.onEndEdit.AddListener(OnInputChanged);
        completeConfirmButton.onClick.AddListener(HideCompletePanel);
    }

    public void Open(string itemName, int pricePerItem)
    {
        Debug.Log($"Open called: {itemName} / {pricePerItem}");

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
        Debug.Log($"구매 확인: {itemName} x{currentCount}");

        // 구매 팝업은 애니메이션 없이 바로 꺼지게
        popupPanel.gameObject.SetActive(false);

        // 구매 완료 패널 표시
        ShowPurchaseCompleteMessage(itemName, currentCount);
    }

    public void Close()
    {
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
        completePanelRect.DOAnchorPos(completePanelStartPos, completeSlideDuration)
        .SetEase(Ease.InCubic)
        .OnComplete(() => purchaseCompletePanel.SetActive(false));
    }

}