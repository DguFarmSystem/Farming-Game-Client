using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MiniGameButtonHandler : MonoBehaviour
{
    private Dictionary<string, (string title, string desciption)> gameInfo;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private string selected;

    public TMP_Text title;
    public TMP_Text description;

    void Awake()
    {
        popupPanel.SetActive(false);

        gameInfo = new Dictionary<string, (string, string)> {
            {"RPS", ("rock paper scissors", "rock paper scissors description")},
            {"CarrotFarm", ("Carrot Farm part time job", "Carrot Farm part time job description")},
            {"Sunlight", ("sunlight game", "sunlight game description")},
        };
    }

    public void OpenPopup(string sceneName) {
        selected = sceneName;
        (string titleValue, string descriptionValue) = gameInfo[selected];
        title.text = titleValue;
        description.text = descriptionValue;
        popupPanel.SetActive(true);
        Debug.Log($"{selected} is selected");
    }

    public void Start()
    {
        if (SceneLoader.Instance == null) {
            Debug.Log("SceneLoader.Instance is null");
            return;
        }
        if (string.IsNullOrEmpty(selected)) {
            Debug.Log($"selected nothing");
            return;
        }
        SceneLoader.Instance.GoToMiniGame(selected);
    }

    public void Cancel()
    {
        selected = null;
        popupPanel.SetActive(false);
    }
}
