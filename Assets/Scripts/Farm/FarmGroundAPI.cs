using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FarmGroundAPI
{
    private static string baseUrl = "https://api.dev.farmsystem.kr";
    public static string AccessToken { get; set; } = null;

    private static void SetHeaders(UnityWebRequest req)
    {
        req.SetRequestHeader("Accept", "application/json; charset=UTF-8");
        req.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(AccessToken))
            req.SetRequestHeader("Authorization", "Bearer " + AccessToken);
    }

    [Serializable]
    public class ApiResponse<T>
    {
        public int status;
        public string message;
        public T data;
    }

    [Serializable]
    public class FarmTileDto
    {
        public int x;
        public int y;
        public string status;
        public string plantedAt;
        public int sunlightCount;
        public string plantName;
    }

    public static IEnumerator GetTile(int x, int y, Action<bool, FarmTileDto, string> onDone)
    {
        string url = $"{baseUrl}/api/farm/tile?x={x}&y={y}";
        using (var req = UnityWebRequest.Get(url))
        {
            SetHeaders(req);
            yield return req.SendWebRequest();
            string raw = req.downloadHandler?.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                onDone?.Invoke(false, null, $"{req.responseCode} {req.error} :: {raw}");
                yield break;
            }
            FarmTileDto dto = null;
            try
            {
                var wrapped = JsonUtility.FromJson<ApiResponse<FarmTileDto>>(raw);
                if (wrapped != null && wrapped.data != null) dto = wrapped.data;
                else dto = JsonUtility.FromJson<FarmTileDto>(raw);
            }
            catch { }
            onDone?.Invoke(dto != null, dto, raw);
        }
    }

    public static IEnumerator PatchTile(int x, int y, FarmTileDto body, Action<bool, string> onDone)
    {
        // ❗ URL에 x, y 좌표를 쿼리 파라미터로 추가
        string url = $"{baseUrl}/api/farm/tile?x={x}&y={y}";
        string json = JsonUtility.ToJson(body);
        using (var req = new UnityWebRequest(url, "PATCH"))
        {
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            SetHeaders(req);
            yield return req.SendWebRequest();
            string raw = req.downloadHandler?.text;
            bool ok = (req.result == UnityWebRequest.Result.Success);
            onDone?.Invoke(ok, ok ? raw : $"{req.responseCode} {req.error} :: {raw}");
        }
    }
    
    public static FarmTileDto ToDto(FarmPlotData p)
    {
        return new FarmTileDto
        {
            x = p.x,
            y = p.y,
            status = p.status,
            plantedAt = string.IsNullOrEmpty(p.planted_at) ? null : p.planted_at,
            sunlightCount = p.useSunCount,
            plantName = p.plant_name,
        };
    }
    
    public static void ApplyDtoToPlot(FarmTileDto dto, FarmPlotData p)
    {
        if (dto == null || p == null) return;
        p.x = dto.x;
        p.y = dto.y;
        p.status = string.IsNullOrEmpty(dto.status) ? p.status : dto.status;
        p.planted_at = dto.plantedAt ?? p.planted_at;
        p.useSunCount = dto.sunlightCount;
        if (!string.IsNullOrEmpty(dto.plantName)) p.plant_name = dto.plantName;
    }
    
    public static string NowIsoUtc() => DateTime.UtcNow.ToString("o");
}