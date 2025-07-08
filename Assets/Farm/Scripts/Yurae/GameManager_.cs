using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

public class GameManager_ : MonoBehaviour
{
    // 하드코딩된 토큰
    private string userToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxIiwicm9sZSI6IkFETUlOIiwiaWF0IjoxNzUwOTM2NzQ3LCJleHAiOjE3NTA5NDAzNDd9.h--VaTF3kVugq48hB4VIggtJaDR5Ml4wS_wn1vcYmTs";
    public Text text;

    void Start()
    {
        StartCoroutine(GetUserInfo());
    }

    IEnumerator GetUserInfo()
    {
        string url = "https://api.dev.farmsystem.kr/api/user/today-seed"; // ← 실제 API 주소로 바꿔줘야 해

        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + userToken);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("응답 성공: " + req.downloadHandler.text);
            text.text = "성공";

            // JSON 파싱
            UserInfo info = JsonUtility.FromJson<UserInfo>(req.downloadHandler.text);
        }
        else
        {
            Debug.LogError("API 요청 실패: " + req.error);
        }
    }

    [System.Serializable]
    public class UserInfo
    {
        public int user_id;
        public string nickname;
        public int seed;
    }
}
