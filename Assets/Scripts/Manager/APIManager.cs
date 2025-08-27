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
        // ...
    }

    // Token
    [SerializeField] private string AccessToken;

    private IEnumerator CheckUrlRoutine(string baseUrl)
    {
        UnityWebRequest www = UnityWebRequest.Get(baseUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Base URL 테스트 성공: " + baseUrl);
            Debug.Log("응답: " + www.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Base URL 테스트 실패: " + baseUrl + " / 오류: " + www.error);
        }
    }

    //-------------------------------------
    // Public API Request Methods
    //-------------------------------------

    public void Get(string endPoint, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(GetRequest(baseUrl + endPoint, onSuccess, onError));
    }

    public void Post(string endPoint, string json, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(PostRequest(baseUrl + endPoint, json, onSuccess, onError));
    }
    
    // PUT 메서드 추가
    public void Put(string endPoint, string json, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(PutRequest(baseUrl + endPoint, json, onSuccess, onError));
    }

    // PATCH
    public void Patch(string endPoint, string json, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(PatchRequest(baseUrl + endPoint, json, onSuccess, onError));
    }

    //-------------------------------------
    // Private Coroutine Logic
    //-------------------------------------

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
    
    // PUT 요청을 위한 코루틴 추가
    private IEnumerator PutRequest(string url, string json, Action<string> onSuccess, Action<string> onError)
    {
        UnityWebRequest www = new UnityWebRequest(url, "PUT");
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

    // PATCH
    private IEnumerator PatchRequest(string url, string json, Action<string> onSuccess, Action<string> onError)
    {
        UnityWebRequest www = new UnityWebRequest(url, "PATCH");
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