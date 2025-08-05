using System;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public class PowerStationData
{
    public string uid;
    public string chargeStartTime; // 충전 시작 시간
}

public class PowerStationManager : MonoBehaviour
{
    public static PowerStationManager Instance { get; private set; }

    public PowerStationData stationData;

    [Header("UI Components")]
    public Image tankImage;            // 리소스 이미지 (Lv0~Lv9)
    public TMP_Text percentText;       // "충전량 0%" 등 텍스트
    public Button collectButton;       // 수령 버튼

    [Header("Sprites (Lv0 ~ Lv9)")]
    public Sprite[] levelSprites;      // 총 10개 (index 0 = Lv0, 9 = Lv9)

    [Header("Settings")]
    public string uid; //유저 아이디
    private DateTime chargeStartTime; //충전 시작 시간
    private float chargeCheckInterval = 5f;
    private float chargeCheckTimer = 0f;
    float maxTime = 100f; //최대 충전 시간

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        //StartCoroutine(LoadPowerStationData(uid));

        chargeStartTime = DateTime.Now;

        collectButton.onClick.AddListener(OnCollectSunlight);
        UpdateUI();
    }

    private IEnumerator LoadPowerStationData(string uid)
    {
        string url = $"https://yourserver.com/api/powerstation?uid={uid}";
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string json = req.downloadHandler.text;
            PowerStationData data = JsonUtility.FromJson<PowerStationData>(json);
            this.chargeStartTime = DateTime.Parse(data.chargeStartTime);
            UpdateUI();
        }
        else
        {
            Debug.LogError("Failed to load power station data: " + req.error);
        }
    }

    private IEnumerator UpdateChargeStartTimeToNow()
    {
        string url = $"https://yourserver.com/api/powerstation/{uid}";
        PowerStationData newData = new PowerStationData
        {
            uid = uid,
            chargeStartTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
        };

        string json = JsonUtility.ToJson(newData);
        UnityWebRequest req = UnityWebRequest.Put(url, json);
        req.method = "PATCH";
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogError("충전 시작 시간 갱신 실패: " + req.error);
    }

    private void Update()
    {
        chargeCheckTimer += Time.deltaTime;
        if (chargeCheckTimer >= chargeCheckInterval)
        {
            chargeCheckTimer = 0f;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        TimeSpan elapsed = DateTime.Now - chargeStartTime;
        double totalSeconds = elapsed.TotalSeconds;

        float percent = Mathf.Clamp01((float)(totalSeconds / maxTime)) * 100f;
        int displayPercent = Mathf.FloorToInt(percent);

        // Lv 결정
        int level = GetLevelFromPercent(displayPercent);
        tankImage.sprite = levelSprites[level];

        // 텍스트 업데이트
        percentText.text = $"충전량 {displayPercent}%";


        collectButton.interactable = (displayPercent >= 5);
    }

    private void OnCollectSunlight()
    {
        TimeSpan elapsed = DateTime.Now - chargeStartTime;
        double totalSeconds = elapsed.TotalSeconds;


        float percent = Mathf.Clamp01((float)(totalSeconds / maxTime)) * 100f;
        int availableSunlight = Mathf.FloorToInt(percent) / 2;

        int newSun = availableSunlight;
        if (newSun > 0)
        {
            CurrencyManager.Instance.AddSunlight(newSun);

            // 충전 시간 초기화
            chargeStartTime = DateTime.Now;
            collectButton.interactable = false;

            // UI 바로 갱신
            UpdateUI();

            // 서버에도 반영
            //StartCoroutine(UpdateChargeStartTimeToNow());
        }
    }

    private int GetLevelFromPercent(int percent)
    {
        if (percent >= 100) return 9;
        else if (percent >= 91) return 8;
        else if (percent >= 79) return 7;
        else if (percent >= 57) return 6;
        else if (percent >= 45) return 5;
        else if (percent >= 33) return 4;
        else if (percent >= 20) return 3;
        else if (percent >= 10) return 2;
        else if (percent >= 5) return 1;
        else return 0;
    }
}
