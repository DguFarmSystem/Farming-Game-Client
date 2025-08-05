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
            {"RPS", ("가위바위보", "모두가 아는 그 게임 맞아요.")},
            {"CarrotFarm", ("당근농장 아르바이트", "Carrot Farm part time job description")},
            {"SunshineGame", ("햇빛게임", "sunshine game description")},
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

    public void StartGame()
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

    public void CancelGame()
    {
        selected = null;
        popupPanel.SetActive(false);
    }
}
