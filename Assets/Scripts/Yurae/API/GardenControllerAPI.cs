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
    /// - GardenData.objectData: ???? ObjectData (???? ?? ???? ?? null)
    /// onSuccess: gardens (???? ??????), objects (gardens?? ?????? 1:1, null ????)
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
                        var gardens = resp.data; // ???? ??
                        // ?? garden?? ???? objectData?? 1:1?? ???? (null ????)
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
    /// PATCH /api/garden/update/{x}/{y} (DTO ???? ????)
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
                Converters = { new StringEnumConverter() } // "R90" ?????? enum ????
            };

            string json = JsonConvert.SerializeObject(payload, settings);
            string endpoint = $"/api/garden/update/{x}/{y}";

            Action<string> successWrap = res =>
            {
                onSuccess?.Invoke(res);
                BadgeManager.Instance?.NotifyGardenChangedAndCheckBadgesImmediate();
            };

            APIManager.Instance.Patch(endpoint, json, onSuccess, onError);
        }
        catch (Exception e)
        {
            onError?.Invoke($"Serialize Exception: {e.Message}");
        }
    }

    /// <summary>
    /// PATCH /api/garden/update/{x}/{y} (???? ????????)
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
            objectData = (objectType > 0) ?new Garden.ObjectData
            {
                objectType = objectType,
                rotation = rotation
            }
            : null
        };

        UpdateGardenTile(x, y, req, onSuccess, onError);
    }

    /// <summary>
    /// PATCH /api/garden/update/{x}/{y} (???????? ????: object?? ????)
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
            objectData = null // NullValueHandling.Ignore?? JSON???? "object" ????
        };
        UpdateGardenTile(x, y, req, onSuccess, onError);
    }

    /// <summary>
    /// PATCH /api/garden/rotate
    /// - ???? (x, y)?? ???? object?? ????????
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

    public static void GetFriendGardenDataFromServer(long userId, Action<List<GardenLoadData>, List<ObjectLoadData>> onSuccess, Action<string> onError = null)
    {
        APIManager.Instance.Get(
            $"/api/garden/{userId}",
            result =>
            {
                try
                {
                    var resp = JsonConvert.DeserializeObject<ApiResponse<List<GardenLoadData>>>(result);

                    if (resp != null && resp.status == 200 && resp.data != null)
                    {
                        var gardens = resp.data; // ???? ??
                        // ?? garden?? ???? objectData?? 1:1?? ???? (null ????)
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
}
