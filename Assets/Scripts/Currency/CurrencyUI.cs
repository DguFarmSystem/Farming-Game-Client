using UnityEngine;
using TMPro;
using System.Linq;

public class CurrencyUI : MonoBehaviour
{
    [Header("텍스트들")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI seedCountText;   // ← seedTicket 말고 seedCount 표시
    public TextMeshProUGUI sunlightText;

    private void OnEnable()
    {
        AutoBindIfNull();        // 인스펙터 비면 자동 연결 시도
        StartCoroutine(BindWhenReady());
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= UpdateUI;
    }

    private System.Collections.IEnumerator BindWhenReady()
    {
        // CurrencyManager가 뜰 때까지 대기
        while (CurrencyManager.Instance == null) yield return null;
        CurrencyManager.Instance.OnCurrencyChanged += UpdateUI;
        UpdateUI();
    }

    private void AutoBindIfNull()
    {
        // 하위 오브젝트 이름을 이용해 자동 할당 (원하는 이름으로 바꿔도 됨)
        if (!goldText) goldText = FindTMPContains("gold");
        if (!sunlightText) sunlightText = FindTMPContains("sun") ?? FindTMPContains("sunlight");
        if (!seedCountText) seedCountText = FindTMPContains("seedcount") ?? FindTMPContains("seed");
    }

    private TextMeshProUGUI FindTMPContains(string keyword)
    {
        keyword = keyword.ToLower();
        return GetComponentsInChildren<TextMeshProUGUI>(true)
            .FirstOrDefault(t => t.name.ToLower().Contains(keyword));
    }

    private void UpdateUI()
    {
        var cm = CurrencyManager.Instance;
        if (!cm)
        {
            Debug.LogWarning("[CurrencyUI] CurrencyManager.Instance == null");
            return;
        }

        if (!goldText || !sunlightText || !seedCountText)
        {
            if (!goldText) Debug.LogWarning("[CurrencyUI] goldText is NULL");
            if (!sunlightText) Debug.LogWarning("[CurrencyUI] sunlightText is NULL");
            if (!seedCountText) Debug.LogWarning("[CurrencyUI] seedCountText is NULL");
            return; // null이면 더 진행하지 않음 (NRE 방지)
        }

        goldText.text = cm.gold.ToString("N0");
        sunlightText.text = cm.sunlight.ToString("N0");
        seedCountText.text = cm.seedCount.ToString("N0");
    }

    // 에디터에서 우클릭 > Force Refresh 로 강제 갱신 가능
    [ContextMenu("Force Refresh")]
    private void ForceRefreshInEditor() => UpdateUI();
}
