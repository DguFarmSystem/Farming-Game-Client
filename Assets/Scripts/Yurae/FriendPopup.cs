// Unity
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using UnityEngine.Networking;

[DisallowMultipleComponent]
public class FriendPopup : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    private void Start()
    {
        string query = "¼Û";
        string endpoint = $"/api/user/search?query={UnityWebRequest.EscapeURL(query)}";

        APIManager.Instance.Get(
            endpoint,
            (result) =>
            {
                Debug.Log(result);
            },
            (error) =>
            {
                Debug.Log(error);
            }
        );
    }

    private void Update()
    {

    }
}
