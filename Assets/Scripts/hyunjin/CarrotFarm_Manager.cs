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
    public MinigamePopup minigamePopup;

    [Header("UI")]
    public TMP_Text timerText, scoreText;
    public Button seedButton, waterButton, hammerButton;
    public Image cursorImage;

    [Header("Sprites")]
    public Sprite itemSprite_seed;
    public Sprite itemSprite_water, itemSprite_water_tilted;
    public Sprite itemSprite_hammer, itemSprite_hammer_tilted;

    private float gameDuration, timer;
    private int score;
    private ItemState current;
    private Coroutine cursorAnimCoroutine;

    public void AddScore() { score++; }
    public ItemState GetState() { return current; }
    void Awake() { Instance = this; }

    void Start()
    {
        enabled = false;
        cursorImage.enabled = false;
        gameDuration = 120f;
        score = 0;

        seedButton.onClick.AddListener(() => { SetState(ItemState.SEED); });
        waterButton.onClick.AddListener(() => { SetState(ItemState.WATER); });
        hammerButton.onClick.AddListener(() => { SetState(ItemState.HAMMER); });
        minigamePopup.onStart = () => { StartGame(); };
        minigamePopup.onPause = () => { cursorImage.enabled = false; enabled = false; };
        minigamePopup.onResume = () => { cursorImage.enabled = true; enabled = true; };
    }

    public void SetState(ItemState newState) {
        current = newState;

        StopCursorAnim();
        UpdateCursorSprite();
    }

    void StartGame()
    {
        score = 0;
        timer = gameDuration;
        cursorImage.enabled = true;
        enabled = true;
    }

    void Update()
    {
        if (timer <= 0f) {
            EndGame();
            return;
        }
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
        cursorImage.enabled = false;
        enabled = false;
        minigamePopup.RewardPopup("CarrotFarm", score);
    }
}
