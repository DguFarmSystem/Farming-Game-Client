// Unity
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

using System.Collections.Generic;

[System.Serializable]
public class UserSearchResponse
{
    public int status;
    public string message;
    public UserData[] data;
}

[System.Serializable]
public class UserData
{
    public int id;
    public string name;
    public string profileUrl;
    public string track;
    public int generation;
}

[DisallowMultipleComponent]
public class FriendManager : MonoBehaviour
{
    [Header("Popup Object")]
    [SerializeField] private GameObject friendPopup;

    [Header("UI element")]
    [SerializeField] private Button friendButton;
    [SerializeField] private TMP_InputField inputField;

    [Header("Frien Prefab")]
    [SerializeField] private GameObject friendFramePrefab;
    [SerializeField] private Transform frameParent;

    [Header("Confirm Popup")]
    [SerializeField] private GameObject confirmPopup;
    [SerializeField] private TextMeshProUGUI confirmPopupTMP;
    [SerializeField] private Button cancelButton;

    private List<GameObject> friendFrames;


    private void Start()
    {
        // Button Init
        friendButton.onClick.AddListener(OpenFriendPopup);

        Suggest("");

        // Add Input Field Listner
        inputField.onValueChanged.AddListener(Suggest);

        friendFrames = new List<GameObject>();
        cancelButton.onClick.AddListener(CloseConfirmPopup);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) CloseFriendPopup();
    }

    private void Suggest(string _query)
    {
        Debug.Log(_query);

        string query = _query;
        string endpoint = $"/api/user/suggest?query={UnityWebRequest.EscapeURL(query)}";

        APIManager.Instance.Get(
            endpoint,
            (result) =>
            {
                foreach(GameObject friendFrame in friendFrames)
                {
                    DestroyImmediate(friendFrame.gameObject);
                }

                friendFrames.Clear();

                var parsed = JsonUtility.FromJson<UserSearchResponse>(result);

                foreach (var user in parsed.data)
                {
                    GameObject frame = Instantiate(friendFramePrefab);
                    frame.transform.SetParent(frameParent);
                    frame.transform.localScale = Vector3.one;

                    friendFrames.Add(frame);

                    Friend friend = frame.GetComponent<Friend>();
                    friend.Init(user.id, user.name, user.track);

                    Button button = frame.GetComponent<Button>();
                    button.onClick.AddListener(() => OpenConfirmPopUp(user.name));
                }
                
            },
            (error) =>
            {
                Debug.Log(error);
            }
        );
    }

    public void OpenFriendPopup()
    {
        inputField.text = "";
        friendPopup.SetActive(true);
    }

    public void CloseFriendPopup()
    {
        inputField.text = "";
        friendPopup.SetActive(false);
    }

    public void OpenConfirmPopUp(string _name)
    {
        confirmPopup.SetActive(true);
        confirmPopupTMP.text = _name + "님의 정원으로 이동하시겠습니까?";
    }

    public void CloseConfirmPopup()
    {
        confirmPopup.SetActive(false);
    }
}
