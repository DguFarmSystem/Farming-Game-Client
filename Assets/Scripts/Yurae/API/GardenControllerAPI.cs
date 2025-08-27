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
    /// /api/garden 호출 → data가 배열이므로 List<GardenData>로 파싱
    /// GardenData 리스트와 ObjectData 리스트 둘 다 전달
    /// </summary>
    public static void GetGardenDataFromServer(
        Action<List<Garden.GardenData>, List<Garden.ObjectData>> onSuccess,
        Action<string> onError = null)
    {
        APIManager.Instance.Get(
            "/api/garden",
            result =>
            {
                try
                {
                    var resp = JsonConvert.DeserializeObject<Garden.ApiResponse<List<Garden.GardenData>>>(result);

                    if (resp != null && resp.status == 200 && resp.data != null)
                    {
                        var gardens = resp.data;
                        var objects = gardens.Select(g => g.objectData).ToList();

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
    /// /api/garden/update/{x}/{y} PATCH (직접 DTO 전달)
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
                Converters = { new StringEnumConverter() }
            };

            string json = JsonConvert.SerializeObject(payload, settings);
            string endpoint = $"/api/garden/update/{x}/{y}";

            APIManager.Instance.Patch(
                endpoint,
                json,
                onSuccess,
                onError
            );
        }
        catch (Exception e)
        {
            onError?.Invoke($"Serialize Exception: {e.Message}");
        }
    }

    /// <summary>
    /// /api/garden/update/{x}/{y} PATCH (간편 오버로드)
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
                objectKind = objectType,
                rotation = rotation
            }
        };

        UpdateGardenTile(x, y, req, onSuccess, onError);
    }

    /// <summary>
    /// /api/garden/update/{x}/{y} PATCH (오브젝트 제거하고 싶을 때)
    /// </summary>
    public static void ClearGardenObject(
        int x, int y,
        long tileType,
        Action<string> onSuccess,
        Action<string> onError = null)
    {
        var req = new GardenUpdateRequest { tileType = tileType, objectData = null };
        UpdateGardenTile(x, y, req, onSuccess, onError);
    }
}
