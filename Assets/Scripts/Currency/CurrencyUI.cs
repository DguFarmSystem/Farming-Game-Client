using UnityEngine;
using UnityEngine.UI;

public class CurrencyUI : MonoBehaviour
{
    public Text goldText, seedText, sunText; //각각 골드, 씨앗뽑기, 햇살

    private void Start()
    {   
        //재화 매니저랑 이벤트 연동
        CurrencyManager.Instance.OnCurrencyChanged += UpdateUI;
        UpdateUI();
    }

    private void OnDestroy()
    {
        CurrencyManager.Instance.OnCurrencyChanged -= UpdateUI;
    }

    void UpdateUI()
    {
        goldText.text = CurrencyManager.Instance.gold.ToString();
        seedText.text = CurrencyManager.Instance.seedTicket.ToString();
        sunText.text = CurrencyManager.Instance.sunlight.ToString();
    }
}
