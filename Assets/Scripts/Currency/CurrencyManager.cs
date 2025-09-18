using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

[System.Serializable]
public class CurrencyData
{
    public string uid;
    public int gold;
    public int seedTicket;
    public int sunlight;
    public int seedCount;
}

// ❗ PlayerDataResponse DTO 수정: data 필드를 PlayerCurrencyData 타입으로 변경
[System.Serializable]
public class PlayerDataResponse
{
    public int status;
    public string message;
    public PlayerCurrencyData data;
}

// PlayerCurrencyData DTO 필드명 수정: 서버 응답과 대소문자가 일치하도록 변경
[System.Serializable]
public class PlayerCurrencyData
{
    public int seedTicket;
    public int gold;
    public int sunlight;
    public int seedCount;
}

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private string uid;
    public int seedTicket = 0;
    public int gold = 0;
    public int sunlight = 0;
    public int seedCount = 0;

    public event Action OnCurrencyChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(LoadCurrencyFromServer());
    }

    public IEnumerator LoadCurrencyFromServer()
    {
        string url = "/api/player";
        
        bool done = false;
        string rawResponse = null;
        string error = null;

        APIManager.Instance.Get(url,
            (response) => { rawResponse = response; done = true; },
            (err) => { error = err; done = true; }
        );

        while (!done) yield return null;

        Debug.Log($"서버 응답 (원본): {rawResponse}");

        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError("재화 로딩 실패: " + error);
            SetDefaultCurrencyValues();
            yield break;
        }

        try
        {
            var responseData = JsonUtility.FromJson<PlayerDataResponse>(rawResponse);
            
            if (responseData == null || responseData.data == null)
            {
                Debug.LogWarning("서버 응답에 재화 데이터가 없거나 파싱 실패. 초기 재화 생성 로직을 실행합니다.");
                SetDefaultCurrencyValues();
                yield break;
            }

            gold = responseData.data.gold;
            seedTicket = responseData.data.seedTicket;
            sunlight = responseData.data.sunlight;
            seedCount = responseData.data.seedCount;
            
            OnCurrencyChanged?.Invoke();
            Debug.Log("재화 로딩 성공!");
        }
        catch (Exception ex)
        {
            Debug.LogError("재화 로딩 실패 (JSON 파싱 오류): " + ex.Message + " | 원본 응답: " + rawResponse);
            SetDefaultCurrencyValues();
        }
    }
    
    private void SetDefaultCurrencyValues()
    {
        gold = 1000;
        seedTicket = 10;
        sunlight = 0;
        seedCount = 5;
        uid = "default_uid";
        
        OnCurrencyChanged?.Invoke();
        
        StartCoroutine(SaveCurrencyToServer());
    }

    public IEnumerator SaveCurrencyToServer()
    {
        PlayerCurrencyData data = new PlayerCurrencyData()
        {
            seedTicket = seedTicket,
            gold = gold,
            sunlight = sunlight,
            seedCount = seedCount
        };

        string json = JsonUtility.ToJson(data);
        string url = "/api/player/currency";

        bool done = false;
        string error = null;
        
        APIManager.Instance.Patch(url, json,
            (response) =>
            {
                Debug.Log("재화 저장 성공: " + response);
                done = true;
            },
            (err) =>
            {
                error = err;
                done = true;
            }
        );

        while (!done) yield return null;

        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError("재화 저장 실패: " + error);
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        OnCurrencyChanged?.Invoke();
        StartCoroutine(SaveCurrencyToServer());
    }

    public void AddSeedTicket(int amount)
    {
        seedTicket += amount;
        OnCurrencyChanged?.Invoke();
        StartCoroutine(SaveCurrencyToServer());
    }

    public void AddSunlight(int amount)
    {
        sunlight += amount;
        OnCurrencyChanged?.Invoke();
        StartCoroutine(SaveCurrencyToServer());
    }
    
    public void AddSeedCount(int amount)
    {
        seedCount += amount;
        OnCurrencyChanged?.Invoke();
        StartCoroutine(SaveCurrencyToServer());
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            OnCurrencyChanged?.Invoke();
            StartCoroutine(SaveCurrencyToServer());
            return true;
        }
        return false;
    }

    public bool SpendSeedTicket(int amount)
    {
        if (seedTicket >= amount)
        {
            seedTicket -= amount;
            OnCurrencyChanged?.Invoke();
            StartCoroutine(SaveCurrencyToServer());
            return true;
        }
        return false;
    }

    public bool SpendSunlight(int amount)
    {
        if (sunlight >= amount)
        {
            sunlight -= amount;
            OnCurrencyChanged?.Invoke();
            StartCoroutine(SaveCurrencyToServer());
            return true;
        }
        return false;
    }
    
    public bool SpendSeedCount(int amount)
    {
        if (seedCount >= amount)
        {
            seedCount -= amount;
            OnCurrencyChanged?.Invoke();
            StartCoroutine(SaveCurrencyToServer());
            return true;
        }
        return false;
    }
}