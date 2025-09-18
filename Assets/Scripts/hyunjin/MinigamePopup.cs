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
    public Button goButton;
    public Button stopButton;

    [Header("Reward")]
    public GameObject rewardPopup;
    public TMP_Text rewardText;
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
        goButton.onClick.AddListener(NextRound);
        exitButton_reward.onClick.AddListener(ExitGame);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            PauseGame();
    }

    void StartGame()
    {
        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        desciptionPopup.SetActive(false);
        bettingPopup.SetActive(false);
        rewardPopup.SetActive(false);
        onStart?.Invoke();
    }

    void PauseGame()
    {
        if (isPaused) return;

        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        Time.timeScale = 0f;
        isPaused = true;
        escPopup.SetActive(true);
        onPause?.Invoke();
    }

    void ResumeGame()
    {
        if (!isPaused) return;

        GameManager.Sound.SFXPlay("SFX_ButtonCancle");
        Time.timeScale = 1f;
        isPaused = false;
        escPopup.gameObject.SetActive(false);
        onResume?.Invoke();
    }

    void NextRound()
    {
        GameManager.Sound.SFXPlay("SFX_GameStart");
        desciptionPopup.SetActive(false);
        bettingPopup.SetActive(false);
        rewardPopup.SetActive(false);
        onNextRound?.Invoke();
    }

    void ExitGame()
    {
        GameManager.Sound.SFXPlay("SFX_ButtonClick");
        onExit?.Invoke();
        Time.timeScale = 1f;
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.ExitMiniGame();
        else
            Debug.Log("SceneLoader.Instance is null");
    }

    // 외부 호출

    public void BettingPopup(string game, int round, int ifLose, int ifWin, int currentGold)
    {
        if (game != "RPS")
            GameManager.Sound.SFXPlay("SFX_GameOver");
        betRewardText.text = $"골드를 획득했습니다!\n획득 골드 : {currentGold}G";
        bettingText.text = $"{round+1} round\n도전 실패 시 : {ifLose}G\n도전 성공 시 : +{ifWin}G";

            // 저장 후 종료
        stopButton.onClick.RemoveAllListeners();
        stopButton.onClick.AddListener(() =>
        {
            CurrencyManager.Instance?.AddGold(currentGold);
            bettingPopup.SetActive(false);
            ExitGame();
        });
        bettingPopup.gameObject.SetActive(true);
    }
    
    public void RewardPopup(string game, int gold)
    {
        if (game != "RPS")
            GameManager.Sound.SFXPlay("SFX_GameOver");
        rewardPopup.gameObject.SetActive(true);
        if (gold == 0)
            rewardText.text = "골드를 획득하지 못했습니다.\n조금만 더 힘내보세요!";
        else
        {
            rewardText.text = $"골드를 획득했습니다!\n획득 골드 : {gold}G";
            CurrencyManager.Instance?.AddGold(gold);
        }
    }

}