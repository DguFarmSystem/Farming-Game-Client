using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SunshineGame_Manager : MonoBehaviour
{
    public MinigamePopup minigamePopup;

    public Transform layout;
    public RectTransform timerMask;
    public float totalTime;
    public RectTransform dragBox;
    public TMP_Text scoreText;

    private bool isGameOver, isPaused;
    private int score;
    private float currentTime;
    private float timerSize;
    private Vector2 startPos, endPos;
    private List<SunshineGame_Sunshine> dragedList = new List<SunshineGame_Sunshine>();
    
    void Start()
    {
        isPaused = false;
        isGameOver = false;
        score = 0;
        totalTime = 120f;
        currentTime = totalTime;
        timerSize = timerMask.rect.height;
        dragBox.gameObject.SetActive(false);
        minigamePopup.onPause = () => { isPaused = true; };
        minigamePopup.onResume = () => { isPaused = false; };
    }

    void Update()
    {
        if (isPaused || isGameOver) return;

        // timer
        if (currentTime <= 0f) {
            isGameOver = true;
            EndGame();
            return;
        }
        currentTime -= Time.deltaTime;
        timerMask.sizeDelta = new Vector2(timerMask.sizeDelta.x, timerSize * (currentTime / totalTime));

        // input
        if (Input.GetMouseButtonDown(0)) { // 1. 마우스 누를 때
            dragBox.gameObject.SetActive(true);
            startPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0)) { // 2. 마우스 누르는 동안
            endPos = Input.mousePosition;
            DrawDragBox();
            SelectObjects();
        }
        if (Input.GetMouseButtonUp(0)) { // 3. 마우스 뗄 때
            Calculate();
            dragBox.gameObject.SetActive(false);
            dragedList.Clear();
        }
    }

    void DrawDragBox()
    {
        Vector2 size = new Vector2(Mathf.Abs(endPos.x-startPos.x), Mathf.Abs(startPos.y-endPos.y));
        Vector2 pos = new Vector2(Mathf.Min(startPos.x, endPos.x), Mathf.Min(startPos.y, endPos.y)) + size/2;
        dragBox.position = pos;
        dragBox.sizeDelta = size;
    }

    void SelectObjects()
    {
        Rect dragRect = GetWorldRect(dragBox);
        dragedList.Clear();

        foreach (Transform child in layout) {
            SunshineGame_Sunshine sun = child.GetComponent<SunshineGame_Sunshine>();
            if (sun == null) continue;

            RectTransform sunRect = child.GetComponent<RectTransform>();
            Rect sunWorldRect = GetWorldRect(sunRect);
            if (dragRect.Overlaps(sunWorldRect, true)) {
                dragedList.Add(sun);
            }
        }
    }

    void Calculate()
    {
        int sum = 0;
        foreach (var sun in dragedList) sum += sun.GetNum();
        if (sum != 10) return;
        foreach (var sun in dragedList) sun.Pop();
        score += dragedList.Count;
        scoreText.text = $"{score}";
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        float width = corners[2].x - corners[0].x;
        float height = corners[2].y - corners[0].y;
        return new Rect(corners[0].x, corners[0].y, width, height);
    }

    void EndGame() {
        minigamePopup.RewardPopup("SunshineGame", score);
    }
}
