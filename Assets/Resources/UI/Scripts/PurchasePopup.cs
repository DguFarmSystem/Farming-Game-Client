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

    private void Awake()
    {
        Debug.Log("Awake: PurchasePopup ???? ??");
        Instance = this;

        shownPos = popupPanel.anchoredPosition;
        hiddenPos = shownPos + new Vector2(0, -Screen.height);
        popupPanel.anchoredPosition = hiddenPos;

        plusButton.onClick.AddListener(() => ChangeCount(1));
        minusButton.onClick.AddListener(() => ChangeCount(-1));
        confirmButton.onClick.AddListener(OnConfirm);
        rejectButton.onClick.AddListener(Close);
        inputField.onEndEdit.AddListener(OnInputChanged);
    }

    public void Open(string itemName, int pricePerItem)
    {
        Debug.Log($"Open called: {itemName} / {pricePerItem}");

        gameObject.SetActive(true);
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
        totalPriceText.text = $"총 금액 : {total}G";
    }

    private void OnConfirm()
    {
        Debug.Log($"구매 확인: {itemName} x{currentCount}");
        Close();
    }

    public void Close()
    {
        popupPanel.DOAnchorPos(hiddenPos, slideDuration).SetEase(Ease.InBack)
            .OnComplete(() => gameObject.SetActive(false));
    }
}
