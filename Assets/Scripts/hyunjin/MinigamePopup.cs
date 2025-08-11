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

    [Header("Betting")]
    public GameObject bettingPopup;
    public TMP_Text betRewardText;
    public TMP_Text bettingText;
    public Button betButton;
    public Button endButton;

    [Header("Reward")]
    public GameObject rewardPopup;
    public TMP_Text rewardText;
    public Button retryButton;
    public Button exitButton_reward;

    // 콜백
    public Action onStart, onResume, onPause, onNextRound, onExit;

    private bool isPaused = false;
    private string pendingGame;
    private int pendingWins;

    void Start()
    {
        desciptionPopup.SetActive(true);
        escPopup.SetActive(false);
        bettingPopup.SetActive(false);
        rewardPopup.SetActive(false);

        startButton.onClick.AddListener(StartGame);
        resumeButton.onClick.AddListener(ResumeGame);
        exitButton_esc.onClick.AddListener(ExitGame);
        betButton.onClick.AddListener(NextRound);
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
        bettingPopup.SetActive(false);
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

    void NextRound()
    {
        desciptionPopup.SetActive(false);
        bettingPopup.SetActive(false);
        rewardPopup.SetActive(false);
        onNextRound?.Invoke();
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

    // 외부 호출

    public void BettingPopup(string game, int round, int ifWin, int ifLose, int currentGold)
    {
        betRewardText.text = $"골드를 획득했습니다!\n획득 골드 : {currentGold}G";
        bettingText.text = $"{round+1} round\n도전 실패 시 : {ifWin}G\n도전 성공 시 : +{ifLose}G";
        endButton.onClick.AddListener(() => { bettingPopup.SetActive(false); ExitGame(); }); // db 저장
        bettingPopup.gameObject.SetActive(true);
    }
    
    public void RewardPopup(string game, int gold)
    {
        rewardPopup.gameObject.SetActive(true);
        if (gold == 0)
            rewardText.text = "골드를 획득하지 못했습니다.\n조금만 더 힘내보세요!";
        else
            rewardText.text = $"골드를 획득했습니다!\n획득 골드 : {gold}G";  // db 저장
    }

}