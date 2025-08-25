// Unity
using UnityEngine;
using UnityEngine.UI;

using TMPro;

[DisallowMultipleComponent]
public class FriendPopup : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    private void Start()
    {
        
    }

    private void Update()
    {
        Debug.Log(inputField.text);
        if (inputField.text == null) return;

        string user = inputField.text;
        //APIManager.Instance.Get("/api/user/" + user, (result) => { Debug.Log(result); }, (error) => { Debug.LogError(error); });
    }
}
