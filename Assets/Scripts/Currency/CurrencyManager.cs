using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class CurrencyData
{
    public string uid; //유저 아이디
    public int gold; //골드
    public int seedTicket; //씨앗 뽑기권
    public int sunlight; // 햇살
    public int seedCount; //씨앗 개수
}

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public string uid; //유저 아이디
    public int seedTicket = 0; //씨앗 뽑기권
    public int gold = 0; //골드
    public int sunlight = 0; //햇살 재화
    public int seedCount = 0; //씨앗 개수

    public event Action OnCurrencyChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 넘겨도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator LoadCurrencyFromServer(string uid)
    {
        //재화 불러오기 url 부분 바꿔야함
        string url = $"https://yourserver.com/api/currency?uid={uid}";
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string json = req.downloadHandler.text;
            CurrencyData data = JsonUtility.FromJson<CurrencyData>(json);
            this.uid = data.uid;
            gold = data.gold;
            seedTicket = data.seedTicket;
            sunlight = data.sunlight;
            seedCount = data.seedCount;

            OnCurrencyChanged?.Invoke();
        }
        else
        {
            Debug.LogError("재화 로딩 실패: " + req.error);
        }
    }
    public IEnumerator SaveCurrencyToServer()
    {
        //재화 저장하기 url 부분 바꿔야함
        CurrencyData data = new CurrencyData()
        {
            uid = this.uid,
            gold = gold,
            seedTicket = seedTicket,
            sunlight = sunlight,
            seedCount = seedCount
        };

        string json = JsonUtility.ToJson(data);
        UnityWebRequest req = UnityWebRequest.Put($"https://yourserver.com/api/currency/{uid}", json);
        req.method = "PATCH";
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogError("재화 저장 실패: " + req.error);
    }

    // 재화 획득 함수들
    public void AddGold(int amount)
    {
        gold += amount;
        OnCurrencyChanged?.Invoke();
        //StartCoroutine(SaveCurrencyToServer());
    }

    public void AddSeedTicket(int amount)
    {
        seedTicket += amount;
        OnCurrencyChanged?.Invoke();
       // StartCoroutine(SaveCurrencyToServer());
    }

    public void AddSunlight(int amount)
    {
        sunlight += amount;
        OnCurrencyChanged?.Invoke();
      //  StartCoroutine(SaveCurrencyToServer());
    }
    public void AddSeedCount(int amount)
    {
        seedCount += amount;
        OnCurrencyChanged?.Invoke();
      //  StartCoroutine(SaveCurrencyToServer());
    }

    //재화 소비 함수들 (성공 여부 반환)
    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            OnCurrencyChanged?.Invoke();
          //  StartCoroutine(SaveCurrencyToServer());
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
         //   StartCoroutine(SaveCurrencyToServer());
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
          //  StartCoroutine(SaveCurrencyToServer());
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
         //   StartCoroutine(SaveCurrencyToServer());
            return true;
        }
        return false;
    }
}
