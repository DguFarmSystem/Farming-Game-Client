using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RPS : MonoBehaviour
{
    public Image currentImage;
    public Sprite rockSprite, paperSprite, scissorsSprite;
    public Image winBox, drawBox, loseBox;
    public Button rockButton, paperButton, scissorsButton;
    public Button desciptionButton, startButton;
    public TMP_Text winCountText;

    public GameResult gameResult;

    private Sprite[] sprites;
    public int round = 0;
    public int winCount = 0;
    public int current = 0;
    public bool isShuffling = false;

    void Start()
    {
        sprites = new Sprite[] { rockSprite, paperSprite, scissorsSprite };
        startButton.onClick.AddListener(StartGame);
        rockButton.onClick.AddListener(() => PlayerSelect(0));
        paperButton.onClick.AddListener(() => PlayerSelect(1));
        scissorsButton.onClick.AddListener(() => PlayerSelect(2));

        winCountText.text = "";
    }

    void StartGame()
    {
        round = 0;
        winCount = 0;
        winCountText.text = $"win count : {winCount}/3";
        winBox.gameObject.SetActive(false);
        loseBox.gameObject.SetActive(false);
        drawBox.gameObject.SetActive(false);
        // 게임 전체 횟수 ++

        StartCoroutine(ShuffleImages());
    }

    IEnumerator ShuffleImages()
    {
        isShuffling = true;
        winBox.gameObject.SetActive(false);
        loseBox.gameObject.SetActive(false);
        drawBox.gameObject.SetActive(false);
        while (isShuffling) {
            currentImage.sprite = sprites[current];
            current = (current + 1) % 3;
            yield return new WaitForSeconds(0.1f);
            // 120초간 버튼 미클릭 시 entrance로
        }
    }

    void PlayerSelect(int player)
    {
        if (!isShuffling) return;

        isShuffling = false;
        round++;
        int com = (current + 2) % 3;

        if (player == com) {
            drawBox.gameObject.SetActive(true);
            Debug.Log($"com: {com} / you: {player} => draw");
        }
        else if ((player == 0 && com == 2) || (player == 1 && com == 0) || (player == 2 && com == 1)) {
            winBox.gameObject.SetActive(true);
            winCount++;
            winCountText.text = $"win count : {winCount}/3";
            Debug.Log($"com: {com} / you: {player} => win");
        }
        else {
            loseBox.gameObject.SetActive(true);
            Debug.Log($"com: {com} / you: {player} => lose");
        }
        StartCoroutine(Hold());
    }

    IEnumerator Hold()
    {
        Debug.Log("hold 3f");
        yield return new WaitForSeconds(3f);
        Debug.Log("hold end");

        if (round == 3)
            gameResult.SaveResult("RPS", winCount);
        else
            StartCoroutine(ShuffleImages());
    }

}
