using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RPS : MonoBehaviour
{
    public MinigamePopup minigamePopup;

    public Image currentImage;
    public Sprite rockSprite, paperSprite, scissorsSprite;
    public Image winBox, drawBox, loseBox;
    public Button rockButton, paperButton, scissorsButton;
    public Image winCountImg;
    public Sprite win0, win1, win2, win3;

    private Sprite[] choiceSprites;
    private Sprite[] winCountSprites;
    public int round = 0;
    public int winCount = 0;
    public int current = 0;
    public bool isShuffling = false;

    private Coroutine autoExitCoroutine;

    void Start()
    {
        choiceSprites = new Sprite[] { rockSprite, paperSprite, scissorsSprite };
        winCountSprites = new Sprite[] { win0, win1, win2, win3 };
        rockButton.onClick.AddListener(() => PlayerSelect(0));
        paperButton.onClick.AddListener(() => PlayerSelect(1));
        scissorsButton.onClick.AddListener(() => PlayerSelect(2));
        winBox.gameObject.SetActive(false);
        loseBox.gameObject.SetActive(false);
        drawBox.gameObject.SetActive(false);
        winCountImg.sprite = winCountSprites[0];

        minigamePopup.onStart = () => { StartGame(); };
        minigamePopup.onExit = () => { StopAllCoroutines(); };
    }

    void StartGame()
    {
        round = 0;
        winCount = 0;
        winCountImg.sprite = winCountSprites[winCount];
        
        // 게임 전체 횟수 ++
        StartCoroutine(ShuffleImages());
    }

    IEnumerator ShuffleImages()
    {
        isShuffling = true;
        winBox.gameObject.SetActive(false);
        loseBox.gameObject.SetActive(false);
        drawBox.gameObject.SetActive(false);

        autoExitCoroutine = StartCoroutine(AutoExitCoroutine());
        while (isShuffling) {
            currentImage.sprite = choiceSprites[current];
            current = (current + 1) % 3;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void PlayerSelect(int player)
    {
        if (!isShuffling) return;
        if (autoExitCoroutine != null) {
            StopCoroutine(autoExitCoroutine);
            autoExitCoroutine = null;
        }

        isShuffling = false;
        round++;
        int com = (current + 2) % 3;

        if (player == com) {
            drawBox.gameObject.SetActive(true);
        }
        else if ((player == 0 && com == 2) || (player == 1 && com == 0) || (player == 2 && com == 1)) {
            winBox.gameObject.SetActive(true);
            winCount++;
            winCountImg.sprite = winCountSprites[winCount];
        }
        else {
            loseBox.gameObject.SetActive(true);
        }
        StartCoroutine(Hold());
    }

    IEnumerator Hold()
    {
        yield return new WaitForSeconds(3f);

        if (round == 3)
            minigamePopup.RewardPopup("RPS", winCount);
        else
            StartCoroutine(ShuffleImages());
    }

    IEnumerator AutoExitCoroutine()
    {
        yield return new WaitForSeconds(120f);
        Debug.Log("미입력 120초 경과, 미니게임 종료");
        SceneLoader.Instance.ExitMiniGame();
    }

}
