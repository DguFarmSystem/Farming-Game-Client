using System;
using System.Collections.Generic;
using System.Linq;
using Garden;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

public class GardenControllerAPI : MonoBehaviour
{
    /// <summary>
    /// GET /api/garden
    /// - data: List<GardenData>
    /// - GardenData.objectData: 단일 ObjectData (없을 수 있음 → null)
    /// onSuccess: gardens (원본 리스트), objects (gardens와 인덱스 1:1, null 포함)
    /// </summary>
    public static void GetGardenDataFromServer(
        Action<List<GardenLoadData>, List<ObjectLoadData>> onSuccess,
        Action<string> onError = null)
    {
        APIManager.Instance.Get(
            "/api/garden",
            result =>
            {
                try
                {
                    var resp = JsonConvert.DeserializeObject<ApiResponse<List<GardenLoadData>>>(result);

                    if (resp != null && resp.status == 200 && resp.data != null)
                    {
                        var gardens = resp.data; // 여러 개
                        // 각 garden의 단일 objectData를 1:1로 모음 (null 가능)
                        var objects = gardens.Select(g => g.loadData).ToList();

                        onSuccess?.Invoke(gardens, objects);
                    }
                    else
                    {
                        onError?.Invoke($"Parsing/Status Error: {result}");
                    }
                }
                catch (Exception e)
                {
                    onError?.Invoke($"Exception: {e.Message}");
                }
            },
            error => onError?.Invoke(error)
        );
    }

    /// <summary>
    /// PATCH /api/garden/update/{x}/{y} (DTO 직접 전달)
    /// </summary>
    public static void UpdateGardenTile(
        int x, int y,
        GardenUpdateRequest payload,
        Action<string> onSuccess,
        Action<string> onError = null)
    {
        try
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = { new StringEnumConverter() } // "R90" 문자열 enum 대응
            };

            string json = JsonConvert.SerializeObject(payload, settings);
            string endpoint = $"/api/garden/update/{x}/{y}";

            APIManager.Instance.Patch(endpoint, json, onSuccess, onError);
        }
        catch (Exception e)
        {
            onError?.Invoke($"Serialize Exception: {e.Message}");
        }
    }

    /// <summary>
    /// PATCH /api/garden/update/{x}/{y} (간편 오버로드)
    /// </summary>
    public static void UpdateGardenTile(
        int x, int y,
        long tileType,
        long objectType,
        RotationEnum rotation,
        Action<string> onSuccess,
        Action<string> onError = null)
    {
        var req = new GardenUpdateRequest
        {
            tileType = tileType,
            objectData = new Garden.ObjectData
            {
                objectType = objectType,
                rotation = rotation
            }
        };

        UpdateGardenTile(x, y, req, onSuccess, onError);
    }

    /// <summary>
    /// PATCH /api/garden/update/{x}/{y} (오브젝트 제거: object를 생략)
    /// </summary>
    public static void ClearGardenObject(
        int x, int y,
        long tileType,
        Action<string> onSuccess,
        Action<string> onError = null)
    {
        var req = new GardenUpdateRequest
        {
            tileType = tileType,
            objectData = null // NullValueHandling.Ignore로 JSON에서 "object" 빠짐
        };
        UpdateGardenTile(x, y, req, onSuccess, onError);
    }

    /// <summary>
    /// PATCH /api/garden/rotate
    /// - 좌표 (x, y)에 있는 object를 회전시킴
    /// </summary>
    public static void RotateGardenObject(
        int x, int y,
        long objectType,
        RotationEnum rotation,
        Action<string> onSuccess,
        Action<string> onError = null)
    {
        try
        {
            var payload = new
            {
                x = x,
                y = y,
                object_type = objectType,
                rotation = rotation.ToString() // "R0", "R90", "R180", ...
            };

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = { new StringEnumConverter() }
            };

            string json = JsonConvert.SerializeObject(payload, settings);
            string endpoint = "/api/garden/rotate";

            APIManager.Instance.Patch(endpoint, json, onSuccess, onError);
        }
        catch (Exception e)
        {
            onError?.Invoke($"Serialize Exception: {e.Message}");
        }
    }
}
