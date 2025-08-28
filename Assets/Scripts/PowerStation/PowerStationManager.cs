using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class PowerStationData
{
    public string chargeStartTime;
}

[System.Serializable]
public class SolarStationDataResponse
{
    public int status;
    public string message;
    public SolarStationDto data;
}

[System.Serializable]
public class SolarStationDto
{
    public string chargeStartTime;
}


public class PowerStationManager : MonoBehaviour
{
    public static PowerStationManager Instance { get; private set; }

    public PowerStationData stationData;

    [Header("UI Components")]
    public Image tankImage;
    public TMP_Text percentText;
    public Button collectButton;
    [Header("Sprites (Lv0 ~ Lv9)")]
    public Sprite[] levelSprites;

    [Header("Settings")]
    public string uid;
    private DateTime chargeStartTime;
    private float chargeCheckInterval = 5f;
    private float chargeCheckTimer = 0f;
    float maxTime = 100f;

    [Header("Sunlight Effect")]
    public GameObject sunEffectPrefab;
    public Transform sunEffectParent;
    public Transform sunTargetUI;
    public int effectCount = 3;
    public float spreadRadius = 100f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(LoadPowerStationData());
        collectButton.onClick.AddListener(OnCollectSunlight);
    }

    private IEnumerator LoadPowerStationData()
    {
        string endPoint = "/api/solarstation";

        bool done = false;
        string rawResponse = null;
        string error = null;

        APIManager.Instance.Get(endPoint,
            (response) => { rawResponse = response; done = true; },
            (err) => { error = err; done = true; }
        );

        while (!done) yield return null;
        
        bool requiresInitialSave = false;
        
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError("Failed to load power station data: " + error);
            chargeStartTime = DateTime.UtcNow;
            requiresInitialSave = true;
        }
        else
        {
            try
            {
                var responseData = JsonUtility.FromJson<SolarStationDataResponse>(rawResponse);
                DateTime parsedTime;

                if (responseData != null && responseData.data != null && 
                    DateTime.TryParse(responseData.data.chargeStartTime, out parsedTime))
                {
                    chargeStartTime = parsedTime.ToUniversalTime();
                }
                else
                {
                    Debug.LogWarning("Server returned no solar station data. Initializing with local time and sending to server.");
                    chargeStartTime = DateTime.UtcNow;
                    requiresInitialSave = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse solar station data: " + ex.Message);
                chargeStartTime = DateTime.UtcNow;
                requiresInitialSave = true;
            }
        }
        
        if (requiresInitialSave)
        {
            yield return StartCoroutine(UpdateChargeStartTimeToServer(true));
        }
        
        UpdateUI();
    }
    
    private IEnumerator UpdateChargeStartTimeToServer(bool isInitialLoad)
    {
        string endPoint = "/api/solarstation/chargetime";

        SolarStationDto newData = new SolarStationDto
        {
            chargeStartTime = chargeStartTime.ToString("o")
        };

        string json = JsonUtility.ToJson(newData);
        
        bool done = false;
        string error = null;
        
        APIManager.Instance.Patch(endPoint, json,
            (response) => {
                Debug.Log("Charge start time updated successfully.");
                done = true;
            },
            (err) => {
                error = err;
                done = true;
            }
        );

        while(!done) yield return null;

        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError("Failed to update charge start time: " + error);
        }
        
        if (isInitialLoad)
        {
            UpdateUI();
        }
    }

    private IEnumerator UpdateChargeStartTimeToServer()
    {
        yield return StartCoroutine(UpdateChargeStartTimeToServer(false));
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
        TimeSpan elapsed = DateTime.UtcNow - chargeStartTime;
        double totalSeconds = elapsed.TotalSeconds;

        float percent = Mathf.Clamp01((float)(totalSeconds / maxTime));
        int displayPercent = Mathf.FloorToInt(percent * 100f);

        int level = GetLevelFromPercent(displayPercent);
        if (level >= 0 && level < levelSprites.Length)
        {
            tankImage.sprite = levelSprites[level];
        }

        percentText.text = $"충전량 {displayPercent}%";
        collectButton.interactable = (displayPercent >= 5);
    }

    private void OnCollectSunlight()
    {
        TimeSpan elapsed = DateTime.UtcNow - chargeStartTime;
        double totalSeconds = elapsed.TotalSeconds;

        float percent = Mathf.Clamp01((float)(totalSeconds / maxTime)) * 100f;
        int availableSunlight = Mathf.FloorToInt(percent) / 2;

        StartCoroutine(PlaySunlightEffect());

        int newSun = availableSunlight;
        if (newSun > 0)
        {
            CurrencyManager.Instance.AddSunlight(newSun);

            chargeStartTime = DateTime.UtcNow;
            StartCoroutine(UpdateChargeStartTimeToServer());

            collectButton.interactable = false;
            UpdateUI();
        }
        GameManager.Sound.SFXPlay("SFX_Powerstation");
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

    private IEnumerator PlaySunlightEffect()
    {
        Vector3 startWorldPos = collectButton.transform.position;

        GameObject[] sunObjects = new GameObject[effectCount];
        RectTransform[] sunRects = new RectTransform[effectCount];
        Vector3[] midPositions = new Vector3[effectCount];

        float midDuration = 0.3f;
        float endDuration = 0.5f;

        for (int i = 0; i < effectCount; i++)
        {
            GameObject sun = Instantiate(sunEffectPrefab, sunEffectParent);
            RectTransform rt = sun.GetComponent<RectTransform>();

            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spreadRadius;
            Vector3 midPos = startWorldPos + (Vector3)randomOffset;

            rt.position = startWorldPos;

            sunObjects[i] = sun;
            sunRects[i] = rt;
            midPositions[i] = midPos;
        }

        float t = 0f;
        while (t < midDuration)
        {
            t += Time.deltaTime;
            float lerp = t / midDuration;
            for (int i = 0; i < effectCount; i++)
            {
                sunRects[i].position = Vector3.Lerp(startWorldPos, midPositions[i], lerp);
            }
            yield return null;
        }

        t = 0f;
        Vector3 endPos = sunTargetUI.position;
        while (t < endDuration)
        {
            t += Time.deltaTime;
            float lerp = t / endDuration;
            for (int i = 0; i < effectCount; i++)
            {
                sunRects[i].position = Vector3.Lerp(midPositions[i], endPos, lerp);
            }
            yield return null;
        }

        for (int i = 0; i < effectCount; i++)
        {
            Destroy(sunObjects[i]);
        }
    }
}