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
    public int userId;
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
    [SerializeField] private Button confirmButton;

    [Header("Grid Manager")]
    [SerializeField] private GridManager gridManager;

    [Header("Off UI")]
    [SerializeField] private GameObject[] offUI;

    [Header("On UI")]
    [SerializeField] private GameObject sceneLoader;
    [SerializeField] private GameObject infoUI;
    [SerializeField] private Button returnButton;

    [Header("Friend Popup")]
    [SerializeField] private TextMeshProUGUI nameTMP;

    [Header("Fade Manager")]
    [SerializeField] private FadeManager fadeManager;

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

        // Init Listener
        confirmButton.onClick.RemoveAllListeners();

        returnButton.onClick.AddListener(ReturnMyGarden);

        sceneLoader = FindFirstObjectByType<SceneLoader>().gameObject;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) CloseFriendPopup();
    }

    private void Suggest(string _query)
    {
        string query = _query;
        string endpoint = $"/api/user/suggest?query={UnityWebRequest.EscapeURL(query)}";

        APIManager.Instance.Get(
            endpoint,
            (result) =>
            {
                foreach (GameObject friendFrame in friendFrames)
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
                    friend.Init(user.userId, user.name, user.track);

                    Button button = frame.GetComponent<Button>();
                    button.onClick.AddListener(() => OpenConfirmPopUp(user.name, user.userId));
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
        CloseConfirmPopup();

        RectTransform rect = frameParent.GetComponent<RectTransform>();

        Vector2 pos = rect.anchoredPosition;
        pos.y = 0;
        rect.anchoredPosition = pos;
    }

    public void OpenConfirmPopUp(string _name, long _id)
    {
        confirmPopup.SetActive(true);
        confirmPopupTMP.text = _name + "?? ???? ?????????";
    }

    public void CloseConfirmPopup()
    {
        confirmPopup.SetActive(false);

        // Init Listener
        confirmButton.onClick.RemoveAllListeners();
    }

    public void GetFriendData(long userID)
    {
        fadeManager.FadeIn();
        gridManager.LoadDataFromServer(true, userID);

        CloseConfirmPopup();
        CloseFriendPopup();

        foreach (GameObject obj in offUI)
        {
            obj.SetActive(false);
        }

        sceneLoader.gameObject.SetActive(false);

        infoUI.SetActive(true);
        returnButton.gameObject.SetActive(true);
    }

    public void ReturnMyGarden()
    {
        fadeManager.FadeIn();
        gridManager.Build();

        foreach (GameObject obj in offUI)
        {
            obj.SetActive(true);
        }

        sceneLoader.gameObject.SetActive(true);

        infoUI.SetActive(false);
        returnButton.gameObject.SetActive(false);
    }
}
