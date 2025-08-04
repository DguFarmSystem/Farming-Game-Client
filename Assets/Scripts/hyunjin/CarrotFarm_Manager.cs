using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public enum ItemState{
    NONE,
    SEED,
    WATER,
    HAMMER
}

public class CarrotFarm_Manager : MonoBehaviour
{
    public static CarrotFarm_Manager Instance;
    public GameResult gameResult;

    [Header("UI")]
    public TMP_Text timerText, scoreText;
    public Button startButton, seedButton, waterButton, hammerButton;
    public Image cursorImage;

    [Header("Sprites")]
    public Sprite itemSprite_seed;
    public Sprite itemSprite_water, itemSprite_water_tilted;
    public Sprite itemSprite_hammer, itemSprite_hammer_tilted;

    private float gameDuration, timer;
    private int score;
    private ItemState current;
    private bool isGameOver;
    private Coroutine cursorAnimCoroutine;

    public void AddScore() { score++; }
    public bool IsGameOver() { return isGameOver; }
    public ItemState GetState() { return current; }
    void Awake() { Instance = this; }

    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        startButton.interactable = true;
        isGameOver = true;
        enabled = false;
        cursorImage.enabled = false;
        gameDuration = 20f;
        score = 0;
    }

    public void SetState(ItemState newState) {
        current = newState;

        StopCursorAnim();
        UpdateCursorSprite();
    }

    void StartGame()
    {
        startButton.interactable = false;
        score = 0;
        timer = gameDuration;
        isGameOver = false;
        enabled = true;
        seedButton.onClick.AddListener(() => { SetState(ItemState.SEED); });
        waterButton.onClick.AddListener(() => { SetState(ItemState.WATER); });
        hammerButton.onClick.AddListener(() => { SetState(ItemState.HAMMER); });
    }

    void Update()
    {
        if (isGameOver) return;
        if (timer <= 0f) EndGame();

        timer -= Time.deltaTime;
        timerText.text = $"{Mathf.Max((int)timer, 0)}";
        scoreText.text = $"{score}";

        // 커서에 아이템 표시
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(cursorImage.canvas.transform as RectTransform, Input.mousePosition, null, out pos);
        cursorImage.rectTransform.localPosition = pos + new Vector2(100, 100);

        // 커서 애니메이션 처리
        if (Input.GetMouseButtonDown(0)) {
            if (current == ItemState.WATER)
                StartCursorAnim(itemSprite_water_tilted, itemSprite_water);
            else if (current == ItemState.HAMMER)
                StartCursorAnim(itemSprite_hammer_tilted, itemSprite_hammer);
        }
        else if (Input.GetMouseButtonUp(0)) {
            StopCursorAnim();
            UpdateCursorSprite();
        }
    }

    void UpdateCursorSprite()
    {
        if (current == ItemState.NONE) {
            cursorImage.enabled = false;
            return;
        }

        if (current == ItemState.SEED) cursorImage.sprite = itemSprite_seed;
        else if (current == ItemState.WATER) cursorImage.sprite = itemSprite_water;
        else if (current == ItemState.HAMMER) cursorImage.sprite = itemSprite_hammer;
        cursorImage.enabled = true;
    }

    void StartCursorAnim(Sprite a, Sprite b)
    {
        StopCursorAnim();
        cursorAnimCoroutine = StartCoroutine(CursorAnimCoroutine(new Sprite[] { a, b }));
    }
    void StopCursorAnim()
    {
        if (cursorAnimCoroutine != null) {
            StopCoroutine(cursorAnimCoroutine);
            cursorAnimCoroutine = null;
        }
    }
    IEnumerator CursorAnimCoroutine(Sprite[] sprites)
    {
        int index = 0;
        while(true) {
            cursorImage.sprite = sprites[index % sprites.Length];
            index++;
            yield return new WaitForSeconds(1f);
        }
    }

    void EndGame()
    {
        isGameOver = true;
        enabled = false;
        cursorImage.enabled = false;
        SetState(ItemState.NONE);
        seedButton.interactable = false;
        waterButton.interactable = false;
        hammerButton.interactable = false;
        gameResult.SaveResult("CarrotFarm", score);
    }
}
