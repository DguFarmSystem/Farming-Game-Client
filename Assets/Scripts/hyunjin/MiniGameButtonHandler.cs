using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

[Serializable]
public class DailyGameGetResponse
{
    public int status;
    public string message;
    public DailyGameData data;
}

[Serializable]
public class DailyGameData
{
    public string gameType;
    public int count;
}

public class MiniGameButtonHandler : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private string selected;
    private Dictionary<string, (string title, string description, string gameType, int count)> gameInfo;

    public TMP_Text title;
    public TMP_Text description;
    public TMP_Text playCount;
    public Button startButton;

    void Awake()
    {
        popupPanel.SetActive(false);

        gameInfo = new Dictionary<string, (string, string, string, int)> {
            {"RPS", ("가위바위보", "모두가 아는 그 게임 맞아요.", "rock", 3)},
            {"CarrotFarm", ("당근농장 아르바이트", "Carrot Farm part time job description", "carrot", 3)},
            {"SunshineGame", ("햇빛게임", "sunshine game description", "sunlight", 3)},
        };
    }

    void Start()
    {
        foreach(var kv in gameInfo)
        {
            string sceneName = kv.Key;
            var info = kv.Value;

            APIManager.Instance.Get( $"/api/daily-game/{info.gameType}",
                onSuccess: (json) =>
                {
                    try
                    {
                        var res = JsonUtility.FromJson<DailyGameGetResponse>(json);
                        if (res != null && res.data != null)
                            gameInfo[sceneName] = (info.title, info.description, info.gameType, Mathf.Clamp(res.data.count, 0, 3));
                        else
                            Debug.LogWarning($"daily-game GET failed");
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"daily-game GET parsing error : {e.Message}");
                    }
                },
                onError: (err) => Debug.LogWarning($"daily-game GET error : {err}")
            );
        }
    }

    public void OpenPopup(string sceneName) {
        selected = sceneName;
        if (gameInfo.TryGetValue(selected, out var info))
        {
            title.text = info.title;
            description.text = info.description;
            playCount.text = $"플레이 가능 횟수 : {info.count}/3";
            startButton.interactable = (info.count > 0);
        }
        else
        {
            title.text = selected;
            description.text = string.Empty;
            playCount.text = $"플레이 가능 횟수 : 3/3";
        }
        
        popupPanel.SetActive(true);
        GameManager.Sound.SFXPlay("SFX_ButtonClick");
    }

    public void StartGame()
    {
        if (SceneLoader.Instance == null) {
            Debug.Log("SceneLoader.Instance is null");
            return;
        }
        if (string.IsNullOrEmpty(selected)) {
            Debug.Log($"selected nothing");
            return;
        }
        if (!gameInfo.TryGetValue(selected, out var info))
        {
            Debug.Log($"gameInfo not found for {selected}");
            return;
        }

        var body = new DailyGameUseRequest { gameType = info.gameType };
        string json = JsonUtility.ToJson(body);

        APIManager.Instance.Patch(
            "/api/daily-game/use",
            json,
            onSuccess: (_) =>
            {
            },
            onError: (err) =>
            {
                if (!string.IsNullOrEmpty(err) && err.Contains("400"))
                    Debug.LogWarning("일일 게임 제한 초과");
                else
                    Debug.LogWarning($"daily-game/use PATCH error : {err}");

                popupPanel.SetActive(false);
            }
        );
        GameManager.Sound.SFXPlay("SFX_GameStart");
        SceneLoader.Instance.GoToMiniGame(selected);
    }

    public void CancelGame()
    {
        selected = null;
        popupPanel.SetActive(false);
        GameManager.Sound.SFXPlay("SFX_ButtonCancle");
    }

    [Serializable]
    private class DailyGameUseRequest { public string gameType; }

}
