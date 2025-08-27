using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemSlot : MonoBehaviour
{
    [Header("Texts / Icon")]
    public TMP_Text textName;
    public TMP_Text textPrice;
    public Image icon;

    [Header("Upgrade UI (optional)")]
    [SerializeField] private TMP_Text textProgress;   // "1/3", "2/3", "MAX"

    [Header("Upgrade Settings")]
    [SerializeField] private string upgradeResourceKey = "Tilemap_Upgrade";
    [SerializeField] private int upgradeMaxCalls = 3;

    private string PrefsKey => $"upgrade_calls_{upgradeResourceKey}";

    public static event System.Action OnUpgradeProgressChanged;
    public static void RaiseUpgradeProgressChanged() => OnUpgradeProgressChanged?.Invoke();

    // 슬롯 데이터
    public string itemName;
    public int price;
    private string resourceKey;

    private void OnEnable()
    {
        OnUpgradeProgressChanged += RefreshUpgradeStatus;
        RefreshUpgradeStatus();
    }

    private void OnDisable()
    {
        OnUpgradeProgressChanged -= RefreshUpgradeStatus;
    }

    public void Init(ShopItemEntry entry)
    {
        if (entry == null) return;

        itemName = entry.itemName;
        price = entry.price;
        resourceKey = entry.resourceKey;

        if (textName) textName.text = entry.itemName;
        if (textPrice) textPrice.text = entry.price.ToString();
        if (icon) icon.sprite = entry.icon;

        RefreshUpgradeStatus();
    }

    public void SetResourceKey(string key)
    {
        resourceKey = key;
        RefreshUpgradeStatus();
    }

    public void OnClick()
    {
        GameManager.Sound.SFXPlay("SFX_ButtonClick");

        if (IsUpgradeItem() && IsUpgradeMaxed())
        {
            GameManager.Sound.SFXPlay("SFX_ButtonCancle");
            Debug.Log("[ShopItemSlot] Upgrade is MAX → suppress popup.");
            return;
        }

        if (!string.IsNullOrEmpty(resourceKey))
            PurchasePopup.Instance.Open(itemName, price, resourceKey);
        else
            PurchasePopup.Instance.Open(itemName, price);
    }

    private void RefreshUpgradeStatus()
    {
        if (!IsUpgradeItem())
        {
            if (textProgress) textProgress.text = string.Empty;
            return;
        }

        int max = Mathf.Max(1, upgradeMaxCalls);

        const string LEGACY = "grid_upgrade_calls";
        if (PlayerPrefs.HasKey(LEGACY) && !PlayerPrefs.HasKey(PrefsKey))
        {
            int legacy = Mathf.Clamp(PlayerPrefs.GetInt(LEGACY, 0), 0, max);
            PlayerPrefs.SetInt(PrefsKey, legacy);
            PlayerPrefs.DeleteKey(LEGACY);
            PlayerPrefs.Save();
        }

        int stored = PlayerPrefs.GetInt(PrefsKey, 0);
        int used = Mathf.Clamp(stored, 0, max);
        if (used != stored) { PlayerPrefs.SetInt(PrefsKey, used); PlayerPrefs.Save(); }

        int purchaseCap = Mathf.Max(1, max - 1);
        bool maxForDisplay = used >= purchaseCap;

        if (textProgress)
            textProgress.text = maxForDisplay ? "MAX" : $"{Mathf.Min(used + 1, purchaseCap)}/{max}";
    }

    private bool IsUpgradeItem()
        => !string.IsNullOrEmpty(resourceKey) && resourceKey == upgradeResourceKey;

    private bool IsUpgradeMaxed()
    {
        int max = Mathf.Max(1, upgradeMaxCalls);
        int used = Mathf.Clamp(PlayerPrefs.GetInt(PrefsKey, 0), 0, max);

        int purchaseCap = Mathf.Max(1, max - 1);
        return used >= purchaseCap;
    }

}
