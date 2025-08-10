using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class MinigamePopup : MonoBehaviour
{
    [Header("Description")]
    public GameObject desciptionPopup;
    public Button startButton;

    [Header("ESC")]
    public GameObject escPopup;
    public Button resumeButton;
    public Button exitButton_esc;

    [Header("Reward")]
    public GameObject rewardPopup;
    public TMP_Text rewardText;
    public Button retryButton;
    public Button exitButton_reward;

    // 콜백
    public Action onStart;
    public Action onResume;
    public Action onPause;
    public Action onExit;

    private bool isPaused = false;

    void Start()
    {
        desciptionPopup.SetActive(true);
        escPopup.SetActive(false);
        rewardPopup.SetActive(false);

        startButton.onClick.AddListener(StartGame);
        resumeButton.onClick.AddListener(ResumeGame);
        exitButton_esc.onClick.AddListener(ExitGame);
        retryButton.onClick.AddListener(StartGame);
        exitButton_reward.onClick.AddListener(ExitGame);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            PauseGame();
    }

    void StartGame()
    {
        desciptionPopup.SetActive(false);
        rewardPopup.SetActive(false);
        onStart?.Invoke();
    }

    void PauseGame()
    {
        if (isPaused) return;

        Time.timeScale = 0f;
        isPaused = true;
        escPopup.SetActive(true);
        onPause?.Invoke();
    }

    void ResumeGame()
    {
        if (!isPaused) return;

        Time.timeScale = 1f;
        isPaused = false;
        escPopup.gameObject.SetActive(false);
        onResume?.Invoke();
    }

    void ExitGame()
    {
        onExit?.Invoke();
        Time.timeScale = 1f;
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.ExitMiniGame();
        else
            Debug.Log("SceneLoader.Instance is null");
    }
    
    public void RewardPopup(string game, int score) // 외부호출
    {
        rewardPopup.gameObject.SetActive(true);
        rewardText.text = "골드를 획득하지 못했습니다.\n조금만 더 힘내보세요!";
        
        int gold = 0;
        if (game == "RPS") {
            if (score < 2) return;
            gold = score * 10;
        }
        else if (game == "CarrotFarm" || game == "SunshineGame") {
            if (score < 10) return;
            gold = (score / 10) * 10;
        }
        rewardText.text = $"골드를 획득했습니다!\n획득 골드 : {gold}G";
    }

}
