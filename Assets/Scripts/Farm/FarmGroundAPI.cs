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
        public string plantedAt; // string 타입 유지 (JsonUtility 호환성)
        public int sunlightCount;
        public string plantName;
    }


    public static IEnumerator GetTile(int x, int y, Action<bool, FarmTileDto, string> onDone)
    {
        string endPoint = $"/api/farm/tile?x={x}&y={y}";
        bool done = false;
        FarmTileDto dto = null;
        string error = null;

        APIManager.Instance.Get(endPoint,
            (rawResponse) =>
            {
                try
                {
                    var wrapped = JsonUtility.FromJson<ApiResponse<FarmTileDto>>(rawResponse);
                    if (wrapped != null && wrapped.data != null) dto = wrapped.data;
                    else dto = JsonUtility.FromJson<FarmTileDto>(rawResponse);
                }
                catch { }
                done = true;
            },
            (err) =>
            {
                error = err;
                done = true;
            }
        );

        while (!done) yield return null;
        onDone?.Invoke(dto != null, dto, error);
    }
    
    public static IEnumerator PatchTile(FarmTileDto body, Action<bool, string> onDone)
    {
        string endPoint = "/api/farm/tile";
        string json = JsonUtility.ToJson(body);
        bool done = false;
        string error = null;
        
        APIManager.Instance.Patch(endPoint, json,
            (rawResponse) => {
                done = true;
                onDone?.Invoke(true, rawResponse);
            },
            (err) => {
                error = err;
                done = true;
                onDone?.Invoke(false, error);
            }
        );
        
        while (!done) yield return null;
    }
    
    public static FarmTileDto ToDto(FarmPlotData p)
    {
        return new FarmTileDto
        {
            x = p.x,
            y = p.y,
            status = p.status,
            // ❗ DateTime.MinValue일 경우 null, 아닐 경우 문자열로 변환
            plantedAt = (p.planted_at == DateTime.MinValue) ? null : p.planted_at.ToString("o"),
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
        // ❗ dto.plantedAt이 null이 아니면 파싱하여 할당
        if (!string.IsNullOrEmpty(dto.plantedAt) && DateTime.TryParse(dto.plantedAt, out DateTime parsedTime))
        {
            p.planted_at = parsedTime;
        }
        else
        {
            p.planted_at = default(DateTime);
        }
        p.useSunCount = dto.sunlightCount;
        if (!string.IsNullOrEmpty(dto.plantName)) p.plant_name = dto.plantName;
    }
    
    public static string NowIsoUtc() => DateTime.UtcNow.ToString("o");
}