using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FarmGroundAPI
{
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
            // ❗ 서버로 보낼 때는 항상 UTC로 변환하여 ISO 8601 형식으로 보냅니다.
            plantedAt = (p.planted_at == default(DateTime)) ? null : p.planted_at.ToUniversalTime().ToString("o"),
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
        
        DateTime parsedTime;
        // ❗ 서버에서 받은 시간을 UTC로 명시적으로 파싱합니다.
        // 이것이 9시간 차이를 없애는 핵심 코드입니다.
        if (!string.IsNullOrEmpty(dto.plantedAt) && DateTime.TryParse(dto.plantedAt, null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out parsedTime))
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