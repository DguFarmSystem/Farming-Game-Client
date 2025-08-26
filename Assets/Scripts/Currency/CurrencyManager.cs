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

[System.Serializable]
public class PlayerDataResponse
{
    public int status;
    public string message;
    public string data;
}

// PlayerCurrencyData DTO에 seedCount 필드 추가
[System.Serializable]
public class PlayerCurrencyData
{
    public int seedticket;
    public int gold;
    public int sunlight;
    public int seedCount; // 새로운 필드 추가
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
            yield break;
        }

        try
        {
            var responseData = JsonUtility.FromJson<PlayerDataResponse>(rawResponse);
            
            if (responseData == null)
            {
                Debug.LogError("서버 응답 파싱 실패 (PlayerDataResponse). 응답 형식을 확인하세요.");
                yield break;
            }

            if (string.IsNullOrEmpty(responseData.data) || responseData.data.Trim() == "null")
            {
                Debug.LogWarning("서버 응답에 재화 데이터가 없습니다. JSON 파싱을 건너뜁니다.");
                yield break;
            }

            // data 필드가 JSON 문자열이므로 다시 파싱
            PlayerCurrencyData playerData = JsonUtility.FromJson<PlayerCurrencyData>(responseData.data);
            
            if (playerData == null)
            {
                Debug.LogError("재화 데이터 파싱 실패 (PlayerCurrencyData). data 필드 내용 확인 필요.");
                yield break;
            }

            // 모든 재화 값을 로컬 변수에 할당
            gold = playerData.gold;
            seedTicket = playerData.seedticket; // 필드명 변경에 주의
            sunlight = playerData.sunlight;
            seedCount = playerData.seedCount; // 새롭게 추가된 재화 값 할당
            
            OnCurrencyChanged?.Invoke();
            Debug.Log("재화 로딩 성공!");
        }
        catch (Exception ex)
        {
            Debug.LogError("재화 로딩 실패 (JSON 파싱 오류): " + ex.Message + " | 원본 응답: " + rawResponse);
        }
    }
    
    public IEnumerator SaveCurrencyToServer()
    {
        // 서버가 요구하는 형식의 데이터 객체에 모든 재화 값 포함
        PlayerCurrencyData data = new PlayerCurrencyData()
        {
            gold = gold,
            seedticket = seedTicket,
            sunlight = sunlight,
            seedCount = seedCount // 새로운 필드 포함
        };

        string json = JsonUtility.ToJson(data);
        string url = "/api/player/update";

        bool done = false;
        string error = null;

        APIManager.Instance.Put(url, json,
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
        StartCoroutine(SaveCurrencyToServer()); // seedCount가 추가되면 저장 로직도 추가
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
            StartCoroutine(SaveCurrencyToServer()); // seedCount가 추가되면 저장 로직도 추가
            return true;
        }
        return false;
    }
}