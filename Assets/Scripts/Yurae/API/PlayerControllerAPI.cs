// System
using System;

// Unity
using UnityEngine;

public class PlayerControllerAPI : MonoBehaviour
{
    public static void GetPlayerDataFromServer(Action<Player.PlayerData> onSuccess, Action<string> onError = null)
    {
        APIManager.Instance.Get(
            "/api/player",
            result =>
            {
                var response = JsonUtility.FromJson<Player.PlayerResponse>(result);
                if (response != null && response.data != null)
                    onSuccess?.Invoke(response.data);
                else
                    onError?.Invoke("Parsing Error");
            },
            error => onError?.Invoke(error)
        );
    }
}
