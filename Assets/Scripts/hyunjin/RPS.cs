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

    private Sprite[] choiceSprites;
    public int round = 0;
    public int winCount = 0;
    public int current = 0;
    public int result = 0; // -1패, 0무, 1승
    public int currentGold = 0;
    public bool isShuffling = false;

    private Coroutine autoExitCoroutine;
    private AudioSource shuffleSFX;

    void Start()
    {
        choiceSprites = new Sprite[] { rockSprite, paperSprite, scissorsSprite };
        rockButton.onClick.AddListener(() => PlayerSelect(0));
        paperButton.onClick.AddListener(() => PlayerSelect(1));
        scissorsButton.onClick.AddListener(() => PlayerSelect(2));
        winBox.gameObject.SetActive(false);
        loseBox.gameObject.SetActive(false);
        drawBox.gameObject.SetActive(false);

        minigamePopup.onStart = () => { StartGame(); };
        minigamePopup.onNextRound = () => { NextRound(); };
        minigamePopup.onExit = () => { StopAllCoroutines(); };
    }

    void StartGame()
    {
        round = 1;
        winCount = 0;
        current = 0;
        result = 0;
        currentGold = 0;
        StartCoroutine(ShuffleImages());
    }

    void NextRound()
    {
        round++;
        StartCoroutine(ShuffleImages());
    }

    IEnumerator ShuffleImages()
    {
        isShuffling = true;
        winBox.gameObject.SetActive(false);
        loseBox.gameObject.SetActive(false);
        drawBox.gameObject.SetActive(false);

        if (shuffleSFX == null || !shuffleSFX.isPlaying)
            shuffleSFX = GameManager.Sound.SFXPlay("SFX_PSR", true);
        autoExitCoroutine = StartCoroutine(AutoExitCoroutine());
        while (isShuffling) {
            currentImage.sprite = choiceSprites[current];
            current = (current + 1) % 3;
            yield return new WaitForSeconds(0.1f);
        }
        GameManager.Sound.SFXStop(shuffleSFX);
        shuffleSFX = null;
    }

    void PlayerSelect(int player)
    {
        if (!isShuffling) return;
        if (autoExitCoroutine != null) {
            StopCoroutine(autoExitCoroutine);
            autoExitCoroutine = null;
        }

        GameManager.Sound.SFXPlay("SFX_PSR_Select");
        isShuffling = false;
        int com = (current + 2) % 3;

        if (player == com) {
            result = 0;
            drawBox.gameObject.SetActive(true);
        }
        else if ((player == 0 && com == 2) || (player == 1 && com == 0) || (player == 2 && com == 1)) {
            result = 1;
            winCount++;
            winBox.gameObject.SetActive(true);
        }
        else {
            result = -1;
            loseBox.gameObject.SetActive(true);
        }

        if (player == com)
            StartCoroutine(Hold(() => {StartCoroutine(ShuffleImages());}));
        else
            StartCoroutine(Hold(() => {Result();}));
    }

    IEnumerator Hold(System.Action callback)
    {
        yield return new WaitForSeconds(2f);
        callback?.Invoke();
    }

    void Result()
    {
        GameManager.Sound.SFXPlay((result == 1 ? "SFX_PSR_Clear" : "SFX_PSR_Lose"));
        if (round == 1 && result < 1) currentGold = 0;
        else if (round == 1 && result == 1) currentGold = 10;
        else if (round == 2 && result < 1) currentGold = 5;
        else if (round == 2 && result == 1) currentGold = 20;
        else if (round == 3 && result < 1) currentGold = 10;
        else if (round == 3 && result == 1) currentGold = 40;

        if (result == 1 && winCount < 3) {
            minigamePopup.BettingPopup("RPS", round, round==1 ? -5 : -10, round==1 ? 20 : 40, currentGold);
        }
        else { // 지거나 완승
            minigamePopup.RewardPopup("RPS", currentGold);
        }
    }

    IEnumerator AutoExitCoroutine()
    {
        yield return new WaitForSeconds(120f);
        Debug.Log("미입력 120초 경과, 미니게임 종료");
        SceneLoader.Instance.ExitMiniGame();
    }

}
/*
1도전 시 : 0       vs 10
2도전 시 : 5(-5)   vs 20(+10)
3도전 시 : 10(-10) vs 40(+20)
*/