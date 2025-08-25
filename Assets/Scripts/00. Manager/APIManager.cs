// Unity
using UnityEngine;
using UnityEngine.Networking;

// System
using System;
using System.Collections;

public class APIManager : MonoBehaviour
{
    #region Singleton
    public static APIManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    #endregion

    private string baseUrl = "https://api.dev.farmsystem.kr";

    private void Start()
    {
        StartCoroutine(CheckUrlRoutine(baseUrl));

        // Test Code
        Get(
        "/api/cheer",
        (result) => {
            Debug.Log("????: " + result);
        },
        (error) => {
            Debug.LogError("????: " + error);
        }
        );
    }

    // Token
    [SerializeField] private string AccessToken;

    private IEnumerator CheckUrlRoutine(string baseUrl)
    {
        UnityWebRequest www = UnityWebRequest.Get(baseUrl);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Base URL ???? ????: " + baseUrl);
            Debug.Log("????: " + www.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Base URL ???? ????: " + baseUrl + " / Error: " + www.error);
        }
    }

    public void Get(string endPoint, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(GetRequest(baseUrl + endPoint, onSuccess, onError));
    }

    public void Post(string endPoint, string json, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(PostRequest(baseUrl + endPoint, json, onSuccess, onError));
    }

    private IEnumerator GetRequest(string url, Action<string> onSuccess, Action<string> onError)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);

        if (!string.IsNullOrEmpty(AccessToken))
            www.SetRequestHeader("Authorization", "Bearer " + AccessToken);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(www.downloadHandler.text);
        else
            onError?.Invoke(www.error);
    }

    private IEnumerator PostRequest(string url, string json, Action<string> onSuccess, Action<string> onError)
    {
        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(AccessToken))
            www.SetRequestHeader("Authorization", "Bearer " + AccessToken);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(www.downloadHandler.text);
        else
            onError?.Invoke(www.error);
    }
}
