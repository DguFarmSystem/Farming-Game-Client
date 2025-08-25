// Unity
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FriendManager : MonoBehaviour
{
    [SerializeField] private GameObject friendPopup;
    [SerializeField] private Button friendButton;

    private void Start()
    {
        friendButton.onClick.AddListener(OpenFriendPopup);
    }

    public void OpenFriendPopup()
    {
        friendPopup.SetActive(true);
    }

    public void CloseFriendPopup()
    {
        friendPopup.SetActive(false);
    }
}
