using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

public class GameManager_ : MonoBehaviour
{
    // �ϵ��ڵ��� ��ū
    private string userToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxIiwicm9sZSI6IkFETUlOIiwiaWF0IjoxNzUwOTM2NzQ3LCJleHAiOjE3NTA5NDAzNDd9.h--VaTF3kVugq48hB4VIggtJaDR5Ml4wS_wn1vcYmTs";
    public Text text;

    void Start()
    {
        StartCoroutine(GetUserInfo());
    }

    IEnumerator GetUserInfo()
    {
        string url = "https://api.dev.farmsystem.kr/api/user/today-seed"; // �� ���� API �ּҷ� �ٲ���� ��

        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + userToken);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("���� ����: " + req.downloadHandler.text);
            text.text = "����";

            // JSON �Ľ�
            UserInfo info = JsonUtility.FromJson<UserInfo>(req.downloadHandler.text);
        }
        else
        {
            Debug.LogError("API ��û ����: " + req.error);
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
