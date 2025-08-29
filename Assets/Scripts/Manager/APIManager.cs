// Unity
using UnityEngine;
using UnityEngine.Networking;

// System
using System;
using System.Collections;
using System.Runtime.InteropServices; // DllImport

// JSON
using Newtonsoft.Json.Linq;

public class APIManager : MonoBehaviour
{
    #region Singleton
    public static APIManager Instance;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern string GetCookieJS(string name);
    [DllImport("__Internal")] private static extern string GetLocalStorageJS(string key);
    // (Optional) jslib에 브리지 설치 함수가 있다면 여기서 extern 선언
    // [DllImport("__Internal")] private static extern void InstallUnityAccessTokenBridge(string go, string method);
#endif

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        StartCoroutine(CheckUrlRoutine(baseUrl));

#if UNITY_WEBGL && !UNITY_EDITOR
        // 호스트에서 직접 푸시할 경우: SendMessage("APIManager","SetAccessTokenFromJS", token)
        AccessToken = TryLoadTokenFromBrowser();
        if (string.IsNullOrEmpty(AccessToken))
            Debug.LogWarning("[API] 브라우저 저장소에서 토큰을 찾지 못했습니다.");
#else
        // 임시 토큰 발급 로직
        StartCoroutine(InitRoutine());
#endif
    }
    #endregion

    void Start()
    {
        // ❗ 플레이어 데이터 로딩 및 초기화 루틴 시작
        StartCoroutine(LoadPlayerRoutine());
    }


    [SerializeField] private string baseUrl = "https://api.dev.farmsystem.kr";
    public string AccessToken = "";

    /// <summary>호스트 페이지에서 토큰을 푸시로 주입</summary>
    public void SetAccessTokenFromJS(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            // "Bearer " 접두사 오면 제거
            if (token.StartsWith("Bearer ")) token = token.Substring(7);
            AccessToken = token.Trim();
            Debug.Log("[API] AccessToken set from JS.");
        }
    }

    /// <summary>WebGL에서 쿠키/로컬스토리지/URL 쿼리 순으로 토큰 자동 로드</summary>
    private string TryLoadTokenFromBrowser()
    {
        string token = null;

    #if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                // 1) localStorage: fauth-storage(JSON) -> state.accessToken
                string raw = GetLocalStorageJS("auth-storage");
                if (!string.IsNullOrEmpty(raw) && raw.TrimStart().StartsWith("{"))
                {
                    try
                    {
                        var jo = JObject.Parse(raw);
                        token = (string)jo.SelectToken("state.accessToken");
                    }
                    catch { /* JSON 파싱 실패 -> 폴백 진행 */ }
                }

                // 2) 다른 localStorage 키
                if (string.IsNullOrEmpty(token)) token = GetLocalStorageJS("accessToken");
                if (string.IsNullOrEmpty(token)) token = GetLocalStorageJS("access_token");

                // 3) 쿠키
                if (string.IsNullOrEmpty(token)) token = GetCookieJS("accessToken");
                if (string.IsNullOrEmpty(token)) token = GetCookieJS("access_token");
                if (string.IsNullOrEmpty(token)) token = GetCookieJS("Authorization");

                // 형태 정리
                if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
                    token = token.Substring(7);

                // 4) URL 쿼리 ?token=... 폴백
                if (string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(Application.absoluteURL))
                    token = GetUrlParam(Application.absoluteURL, "token");

                Debug.Log("[Token] token :" + token);
            }
            
            catch (Exception e)
            {
                Debug.LogWarning("[API] TryLoadTokenFromBrowser error: " + e.Message);
            }
    #endif
        return token;
    }
    
    // 플레이어 데이터 로드 및 초기화 루틴
    private IEnumerator LoadPlayerRoutine()
    {
        bool done = false;
        string error = null;

        Debug.Log("[Player API] 플레이어 데이터 로드 시도...");

        // GET /api/player 엔드포인트 호출
        Get("/api/player",
            (response) =>
            {
                Debug.Log("[Player API] 데이터 로드 성공. " + response);
                done = true;
            },
            (err) =>
            {
                Debug.LogWarning("[Player API] 데이터 로드 실패. 플레이어 초기화 시도. " + err);
                error = err;
                done = true;
            }
        );

        while (!done) yield return null;

        // 데이터 로드 실패 시 (404) 초기화 API 호출
        if (!string.IsNullOrEmpty(error) && error.Contains("404"))
        {
            Debug.Log("[Player API] 플레이어 초기화 POST 요청 전송...");

            bool initDone = false;
            Post("/api/player/init", "{}",
                (response) =>
                {
                    Debug.Log("[Player API] 초기화 성공! " + response);
                    initDone = true;
                },
                (err) =>
                {
                    Debug.LogError("[Player API] 초기화 실패! " + err);
                    initDone = true;
                }
            );

            while (!initDone) yield return null;
        }
    }

    /// <summary>쿼리 파서 (System.Web 대체)</summary>
    private static string GetUrlParam(string url, string key)
    {
        try
        {
            int q = url.IndexOf('?');
            if (q < 0) return null;
            var parts = url.Substring(q + 1).Split('&');
            for (int i = 0; i < parts.Length; i++)
            {
                var kv = parts[i].Split('=');
                if (kv.Length == 2 && kv[0] == key)
                    return Uri.UnescapeDataString(kv[1]);
            }
        }
        catch { }
        return null;
    }

    /// <summary>요청 전에 토큰을 자동 확보(빈 값/만료 시 브라우저에서 재로드)</summary>
    private bool EnsureToken()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (string.IsNullOrEmpty(AccessToken) || IsJwtExpired(AccessToken))
        {
            var t = TryLoadTokenFromBrowser();
            if (!string.IsNullOrEmpty(t))
            {
                AccessToken = t;
            }
            else
            {
                return false;
            }
        }
#endif
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────

    private IEnumerator CheckUrlRoutine(string urlToCheck)
    {
        UnityWebRequest www = UnityWebRequest.Get(urlToCheck);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Base URL 테스트 성공 : " + urlToCheck);
            if (!string.IsNullOrEmpty(www.downloadHandler.text))
                Debug.Log("응답 : " + www.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Base URL 테스트 실패 : " + urlToCheck + " / Error: " + www.error);
        }
    }

    public void Get(string endPoint, Action<string> onSuccess, Action<string> onError = null)
        => StartCoroutine(GetRequest(baseUrl + endPoint, onSuccess, onError));

    public void Post(string endPoint, string json, Action<string> onSuccess, Action<string> onError = null)
        => StartCoroutine(PostRequest(baseUrl + endPoint, json, onSuccess, onError));

    public void Put(string endPoint, string json, Action<string> onSuccess, Action<string> onError = null)
        => StartCoroutine(PutRequest(baseUrl + endPoint, json, onSuccess, onError));

    public void Patch(string endPoint, string json, Action<string> onSuccess, Action<string> onError = null)
        => StartCoroutine(PatchRequest(baseUrl + endPoint, json, onSuccess, onError));

    // ────────────────────────── 임시 토큰 발급(에디터 전용) ──────────────────────────

    private IEnumerator InitRoutine()
    {
        string issuedToken = null;
        yield return StartCoroutine(RequestTempToken(1,
            (token) => { issuedToken = token; },
            (err) => { Debug.LogError(err); }
        ));

        if (!string.IsNullOrEmpty(issuedToken))
        {
            AccessToken = issuedToken;

            yield return StartCoroutine(GetRequest(
                baseUrl + "/api/player",
                (result) => { Debug.Log("GET 성공: " + result); },
                (error) => { Debug.LogError("GET 실패: " + error); }
            ));
        }
    }

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

    [Serializable] private class TokenData { public string accessToken; public string refreshToken; }
    [Serializable] private class TokenResponse { public int status; public string message; public TokenData data; }

    // ────────────────────────── JWT 체크 ──────────────────────────

    private bool IsJwtExpired(string jwt)
    {
        if (string.IsNullOrEmpty(jwt)) return true;
        var parts = jwt.Split('.');
        if (parts.Length < 2) return true;

        string payloadBase64 = parts[1].Replace('-', '+').Replace('_', '/');
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

    // ────────────────────────── 요청 코루틴 ──────────────────────────

    private IEnumerator GetRequest(string url, Action<string> onSuccess, Action<string> onError)
    {
        if (!EnsureToken()) { onError?.Invoke("AccessToken missing/expired"); yield break; }

        var www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Accept", "application/json");
        if (!string.IsNullOrEmpty(AccessToken))
            www.SetRequestHeader("Authorization", "Bearer " + AccessToken);

        yield return www.SendWebRequest();

        var body = www.downloadHandler != null ? www.downloadHandler.text : string.Empty;
        var wwwAuth = www.GetResponseHeader("WWW-Authenticate");
        Debug.Log($"GET {url} => {(long)www.responseCode}, err={www.error}");
        if (!string.IsNullOrEmpty(wwwAuth)) Debug.Log($"WWW-Authenticate: {wwwAuth}");
        if (!string.IsNullOrEmpty(body)) Debug.Log($"Body: {body}");

        if (www.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(body);
        else
            onError?.Invoke(string.IsNullOrEmpty(body) ? www.error : body);
    }

    private IEnumerator PostRequest(string url, string json, Action<string> onSuccess, Action<string> onError)
    {
        if (!EnsureToken()) { onError?.Invoke("AccessToken missing/expired"); yield break; }

        var www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json ?? ""));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");
        if (!string.IsNullOrEmpty(AccessToken))
            www.SetRequestHeader("Authorization", "Bearer " + AccessToken);

        yield return www.SendWebRequest();

        var body = www.downloadHandler != null ? www.downloadHandler.text : string.Empty;
        var wwwAuth = www.GetResponseHeader("WWW-Authenticate");
        Debug.Log($"POST {url} => {(long)www.responseCode}, err={www.error}");
        if (!string.IsNullOrEmpty(wwwAuth)) Debug.Log($"WWW-Authenticate: {wwwAuth}");
        if (!string.IsNullOrEmpty(body)) Debug.Log($"Body: {body}");

        if (www.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(body);
        else
            onError?.Invoke(string.IsNullOrEmpty(body) ? www.error : body);
    }

    private IEnumerator PutRequest(string url, string json, Action<string> onSuccess, Action<string> onError)
    {
        if (!EnsureToken()) { onError?.Invoke("AccessToken missing/expired"); yield break; }

        var www = new UnityWebRequest(url, "PUT");
        www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json ?? ""));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");
        if (!string.IsNullOrEmpty(AccessToken))
            www.SetRequestHeader("Authorization", "Bearer " + AccessToken);

        yield return www.SendWebRequest();

        var body = www.downloadHandler != null ? www.downloadHandler.text : string.Empty;
        var wwwAuth = www.GetResponseHeader("WWW-Authenticate");
        Debug.Log($"PUT {url} => {(long)www.responseCode}, err={www.error}");
        if (!string.IsNullOrEmpty(wwwAuth)) Debug.Log($"WWW-Authenticate: {wwwAuth}");
        if (!string.IsNullOrEmpty(body)) Debug.Log($"Body: {body}");

        if (www.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(body);
        else
            onError?.Invoke(string.IsNullOrEmpty(body) ? www.error : body);
    }

    private IEnumerator PatchRequest(string url, string json, Action<string> onSuccess, Action<string> onError)
    {
        if (!EnsureToken()) { onError?.Invoke("AccessToken missing/expired"); yield break; }

        var www = new UnityWebRequest(url, "PATCH");
        www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json ?? ""));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");
        if (!string.IsNullOrEmpty(AccessToken))
            www.SetRequestHeader("Authorization", "Bearer " + AccessToken);

        yield return www.SendWebRequest();

        var body = www.downloadHandler != null ? www.downloadHandler.text : string.Empty;
        var wwwAuth = www.GetResponseHeader("WWW-Authenticate");
        Debug.Log($"PATCH {url} => {(long)www.responseCode}, err={www.error}");
        if (!string.IsNullOrEmpty(wwwAuth)) Debug.Log($"WWW-Authenticate: {wwwAuth}");
        if (!string.IsNullOrEmpty(body)) Debug.Log($"Body: {body}");

        if (www.result == UnityWebRequest.Result.Success ||
            (www.responseCode >= 200 && www.responseCode < 300))
            onSuccess?.Invoke(body);
        else
            onError?.Invoke(string.IsNullOrEmpty(body) ? www.error : body);
    }

    // ────────────────────────── 기타 ──────────────────────────

    public string getAccessToken() => AccessToken;
}
