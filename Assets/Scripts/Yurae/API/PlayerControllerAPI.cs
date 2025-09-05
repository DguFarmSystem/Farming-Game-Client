// System
using System;

// Unity
using UnityEngine;

using System.Collections;

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

    [Serializable] private class PlayerLevelPatchReq { public int newLevel; }

    public static IEnumerator CoUpdatePlayerLevelOnServer(int newLevel, Action onSuccess, Action<string> onError)
    {
        if (APIManager.Instance == null)
        {
            onError?.Invoke("APIManager.Instance is null");
            yield break;
        }

        string body = JsonUtility.ToJson(new PlayerLevelPatchReq { newLevel = newLevel });

        bool done = false; bool ok = false; string err = null;
        APIManager.Instance.Patch(
            "/api/player/level",
            body,
            _ => { ok = true; done = true; },
            e => { err = e; done = true; }
        );

        yield return new WaitUntil(() => done);

        if (ok) onSuccess?.Invoke();
        else onError?.Invoke(err ?? "PATCH /api/player/level failed");
    }
}
