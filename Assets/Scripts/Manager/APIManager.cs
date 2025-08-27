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

        // 베이스 URL 헬스체크
        StartCoroutine(CheckUrlRoutine(baseUrl));

        // 시작 시 임시 토큰 발급 후 테스트 API 호출
        StartCoroutine(InitRoutine());
    }
    #endregion

    private string baseUrl = "https://api.dev.farmsystem.kr";

    private void Start()
    {

    }

    // Token
    public string AccessToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxIiwicm9sZSI6IkFETUlOIiwiaWF0IjoxNzU1ODQwMDM2LCJleHAiOjE3NTU4NDM2MzZ9.f4Y16eG9-VUxfkTCocsDpZok00NhwMw1jAjAHQqSpBc";

    private IEnumerator CheckUrlRoutine(string baseUrl)
    {
        UnityWebRequest www = UnityWebRequest.Get(baseUrl);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Base URL 테스트 성공 : " + baseUrl);
            Debug.Log("응답 : " + www.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Base URL 테스트 실패 : " + baseUrl + " / Error: " + www.error);
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

     // ❗ PUT 메서드 추가
    public void Put(string endPoint, string json, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(PutRequest(baseUrl + endPoint, json, onSuccess, onError));
    }

    // 시작 시 임시 토큰 발급
    private IEnumerator InitRoutine()
    {
        // 임시 토큰 발급(userId=1)
        string issuedToken = null;
        yield return StartCoroutine(RequestTempToken(1,
            (token) => { issuedToken = token; },
            (err) => { Debug.LogError(err); }
        ));

        if (!string.IsNullOrEmpty(issuedToken))
        {
            AccessToken = issuedToken; // 발급 토큰 적용

            // 발급된 토큰으로 테스트 GET 호출
            yield return StartCoroutine(GetRequest(
                baseUrl + "/api/player",
                (result) => { Debug.Log("GET 성공: " + result); },
                (error) => { Debug.LogError("GET 실패: " + error); }
            ));
        }
    }

    // 임시 토큰 발급 API 호출
    private IEnumerator RequestTempToken(long userId, Action<string> onSuccess, Action<string> onError)
    {
        string url = $"{baseUrl}/api/auth/token/{userId}";
        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(new byte[0]);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Accept", "application/json");

        yield return www.SendWebRequest();

        string body = www.downloadHandler != null ? www.downloadHandler.text : string.Empty;
        if (www.result == UnityWebRequest.Result.Success)
        {
            // 응답 파싱: { status, message, data:{ accessToken, refreshToken } }
            try
            {
                TokenResponse parsed = JsonUtility.FromJson<TokenResponse>(body);
                string token = parsed != null && parsed.data != null ? parsed.data.accessToken : null;
                if (!string.IsNullOrEmpty(token)) onSuccess?.Invoke(token);
                else onError?.Invoke("임시 토큰 파싱 실패");
            }
            catch (Exception ex)
            {
                onError?.Invoke("임시 토큰 파싱 예외: " + ex.Message);
            }
        }
        else
        {
            onError?.Invoke(string.IsNullOrEmpty(body) ? www.error : body);
        }
    }

    // 임시 토큰 응답 DTO
    [Serializable]
    private class TokenData { public string accessToken; public string refreshToken; }
    [Serializable]
    private class TokenResponse { public int status; public string message; public TokenData data; }

    // JWT 만료 여부 확인 함수 추가
    private bool IsJwtExpired(string jwt)
    {
        if (string.IsNullOrEmpty(jwt)) return true;
        var parts = jwt.Split('.');
        if (parts.Length < 2) return true;

        string payloadBase64 = parts[1]
            .Replace('-', '+')
            .Replace('_', '/');
        payloadBase64 = payloadBase64.PadRight((payloadBase64.Length + 3) / 4 * 4, '=');

        try
        {
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));
            var match = System.Text.RegularExpressions.Regex.Match(json, "\"exp\"\\s*:\\s*(\\d+)");
            if (!match.Success) return false;
            long exp = long.Parse(match.Groups[1].Value);
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return now >= exp;
        }
        catch { return true; }
    }

    // 토큰 만료 사전 체크 및 상세 로깅 추가
    private IEnumerator GetRequest(string url, Action<string> onSuccess, Action<string> onError)
    {
        if (IsJwtExpired(AccessToken))
        {
            onError?.Invoke("AccessToken expired or invalid. 재발급 필요");
            yield break;
        }

        UnityWebRequest www = UnityWebRequest.Get(url);

        // 응답 포맷 명시
        www.SetRequestHeader("Accept", "application/json");

        if (!string.IsNullOrEmpty(AccessToken))
            www.SetRequestHeader("Authorization", "Bearer " + AccessToken);

        yield return www.SendWebRequest();

        string body = www.downloadHandler != null ? www.downloadHandler.text : string.Empty;
        string wwwAuth = www.GetResponseHeader("WWW-Authenticate");

        // 상세 로깅
        Debug.Log($"GET {url} => {(long)www.responseCode}, err={www.error}");
        if (!string.IsNullOrEmpty(wwwAuth)) Debug.Log($"WWW-Authenticate: {wwwAuth}");
        if (!string.IsNullOrEmpty(body)) Debug.Log($"Body: {body}");

        if (www.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(body);
        else
            onError?.Invoke(string.IsNullOrEmpty(body) ? www.error : body);
    }

    // 토큰 만료 사전 체크 및 상세 로깅 추가
    private IEnumerator PostRequest(string url, string json, Action<string> onSuccess, Action<string> onError)
    {
        if (IsJwtExpired(AccessToken))
        {
            onError?.Invoke("AccessToken expired or invalid. 재발급 필요");
            yield break;
        }

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        // 응답 포맷 명시
        www.SetRequestHeader("Accept", "application/json");

        if (!string.IsNullOrEmpty(AccessToken))
            www.SetRequestHeader("Authorization", "Bearer " + AccessToken);

        yield return www.SendWebRequest();

        string body = www.downloadHandler != null ? www.downloadHandler.text : string.Empty;
        string wwwAuth = www.GetResponseHeader("WWW-Authenticate");

        // 상세 로깅
        Debug.Log($"POST {url} => {(long)www.responseCode}, err={www.error}");
        if (!string.IsNullOrEmpty(wwwAuth)) Debug.Log($"WWW-Authenticate: {wwwAuth}");
        if (!string.IsNullOrEmpty(body)) Debug.Log($"Body: {body}");

        if (www.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(body);
        else
            onError?.Invoke(string.IsNullOrEmpty(body) ? www.error : body);
    }

    // ❗ PUT 요청을 위한 코루틴 추가
    private IEnumerator PutRequest(string url, string json, Action<string> onSuccess, Action<string> onError)
    {
        if (IsJwtExpired(AccessToken))
        {
            onError?.Invoke("AccessToken expired or invalid. 재발급 필요");
            yield break;
        }

        UnityWebRequest www = new UnityWebRequest(url, "PUT");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        // 응답 포맷 명시
        www.SetRequestHeader("Accept", "application/json");

        if (!string.IsNullOrEmpty(AccessToken))
            www.SetRequestHeader("Authorization", "Bearer " + AccessToken);

        yield return www.SendWebRequest();

        string body = www.downloadHandler != null ? www.downloadHandler.text : string.Empty;
        string wwwAuth = www.GetResponseHeader("WWW-Authenticate");

        // 상세 로깅
        Debug.Log($"PUT {url} => {(long)www.responseCode}, err={www.error}");
        if (!string.IsNullOrEmpty(wwwAuth)) Debug.Log($"WWW-Authenticate: {wwwAuth}");
        if (!string.IsNullOrEmpty(body)) Debug.Log($"Body: {body}");

        if (www.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(body);
        else
            onError?.Invoke(string.IsNullOrEmpty(body) ? www.error : body);
    }

    public string getAccessToken()
    {
        return AccessToken;
    }

}
