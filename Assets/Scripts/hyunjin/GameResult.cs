using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameResult : MonoBehaviour
{
    public TMP_Text rewardText;
    public Button retryButton;
    public Button exitButton;

    private string game;
    private int score;
    private int gold;

    void Start()
    {
        gameObject.SetActive(false);
        retryButton.onClick.AddListener(OnRetry);
        exitButton.onClick.AddListener(OnExit);
    }

    public void SaveResult(string _game, int _score)
    {
        Debug.Log($"{_game} : {_score}");
        gameObject.SetActive(true);
        gold = 0;
        rewardText.text = "골드를 획득하지 못했습니다.\n조금만 더 힘내보세요!";
        game = _game;
        score = _score;
        CalculateReward();
    }

    void CalculateReward()
    {
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

    void OnRetry()
    {
        Debug.Log("retry button clicked");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnExit()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.ExitMiniGame();
        else
            Debug.Log("SceneLoader.Instance is null");
    }
}
