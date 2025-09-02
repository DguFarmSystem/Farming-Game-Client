using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FarmPopup : MonoBehaviour
{
    public TMP_Text seedCountText; //씨앗 개수
    public TMP_Text sunUseText; //사용할 햇살
    public TMP_Text harvestTime; // 수확 시간

    public Button plusButton; //+ 버튼
    public Button minusButton; //- 버튼
    public Button confirmButton; // 확인 버튼
    public Button cancelButton; // 취소 버튼

    private int useSun = 0; //사용할 햇살 양
    private int maxSun = 10; // 최대 햇살

    private FarmGround targetTile; //타겟 땅
    private string selectedPlantName; //정해진 꽃 이름

    private void Start()
    {
        plusButton.onClick.AddListener(OnPlusClicked);
        minusButton.onClick.AddListener(OnMinusClicked);
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(() => { GameManager.Sound.SFXPlay("SFX_ButtonCancle"); Destroy(gameObject); });
    }

    public void SetTargetTile(FarmGround tile)
    {
        targetTile = tile;


        // 초기 상태
        useSun = 0;
        maxSun = Mathf.Min(10, CurrencyManager.Instance.sunlight);  // 최대 10 개 사용가능
        seedCountText.text = ": " + CurrencyManager.Instance.seedCount.ToString();
        UpdateUI();
    }

    void UpdateUI()
    {
        sunUseText.text = $"{useSun}/{maxSun}";
        int growTime = Mathf.Max(1, 24 - useSun * 2);
        harvestTime.text = $"수확 시간 : {growTime}시간";

        plusButton.interactable = useSun < maxSun; //플러스 금지
        minusButton.interactable = useSun > 0; // 마이너스 금지
    }

    void OnPlusClicked()
    {
        if (useSun < maxSun)
        {
            useSun++;
            UpdateUI();
        }
    }

    void OnMinusClicked()
    {
        if (useSun > 0)
        {
            useSun--;
            UpdateUI();
        }
    }

    void OnConfirm()
    {   
        if (targetTile == null)
    {
        Debug.LogError("[FarmPopup] targetTile이 null입니다. Init이 제대로 안 됐을 가능성 있음");
        return;
    }
        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        // 씨앗 1개 사용 가능할 때만
        if (CurrencyManager.Instance.SpendSeedCount(1))
        {
            CurrencyManager.Instance.SpendSunlight(useSun);
            targetTile.PlantSeed(useSun);  // 씨앗 랜덤 뽑힌거로 확정
        }
        else
        {
            Debug.Log("씨앗이 부족합니다!");
        }

        Destroy(gameObject);
    }
}
