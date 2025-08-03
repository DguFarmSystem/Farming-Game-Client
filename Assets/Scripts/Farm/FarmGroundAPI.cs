using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class FarmGroundAPI
{
    //서버 url 설정해야함
    private static string baseUrl = "https://yourserver.com/api/farmplot";

    public static IEnumerator LoadFarm(string uid, System.Action<List<FarmPlotData>> onLoaded)
    {
        // 더미 초기화 → 서버 없이 빈 밭 생성
        onLoaded?.Invoke(new List<FarmPlotData>());
        yield break;

        string url = $"{baseUrl}?uid={uid}";
        UnityWebRequest req = UnityWebRequest.Get(url);
    
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string json = req.downloadHandler.text;
            List<FarmPlotData> data = JsonHelper.FromJsonArray<FarmPlotData>(json);
            onLoaded?.Invoke(data);
        }
        else
        {
            Debug.LogError("텃밭 불러오기 실패: " + req.error);
            onLoaded?.Invoke(null);
        }
    }

    public static IEnumerator CreatePlot(FarmPlotData data)
    {
        Debug.Log("[FarmAPI] 생성됨: " + data.plot_id);
        yield break;
        string json = JsonUtility.ToJson(data);
        UnityWebRequest req = UnityWebRequest.Put($"{baseUrl}/{data.plot_id}", json);
        req.method = "POST";
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
    }

    public static void UpdatePlot(FarmPlotData data)
    {
        Debug.Log($"[FarmAPI] 업데이트: {data.plot_id}, status={data.status}, plant={data.plant_name}");
        CoroutineRunner.instance.StartCoroutine(UpdatePlotAsync(data));
    }

    private static IEnumerator UpdatePlotAsync(FarmPlotData data)
    {
        string json = JsonUtility.ToJson(data);
        UnityWebRequest req = UnityWebRequest.Put($"{baseUrl}/{data.plot_id}", json);
        req.method = "PATCH";
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
    }
}
