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
    private int sunlight;

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
        sunlight = 0;
        rewardText.text = "fighting";
        game = _game;
        score = _score;
        CalculateReward();
    }

    void CalculateReward()
    {
        if (game == "RPS") {
            if (score == 2) {
                gold = 20;
                sunlight = 20;
                rewardText.text = $"gold: {gold}\nsunlight: {sunlight}";
            }
            else if (score == 3) {
                gold = 30;
                sunlight = 30;
                rewardText.text = $"gold: {gold}\nsunlight: {sunlight}";
            }
        }
        else if (game == "CarrotFarm") {
            gold = (score / 10) * 10;
            rewardText.text = $"gold: {gold}\nsunlight: {sunlight}";
        }
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
