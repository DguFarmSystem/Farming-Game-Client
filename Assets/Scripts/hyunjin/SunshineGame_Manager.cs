using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SunshineGame_Manager : MonoBehaviour
{
    public MinigamePopup minigamePopup;

    public GameObject sunPrefab;
    public Transform startPoint;
    public RectTransform timerMask;
    public float totalTime;
    public RectTransform dragBox;
    public TMP_Text scoreText;

    private int score;
    private float currentTime;
    private float timerSize;
    private Vector2 startPos, endPos;
    private List<SunshineGame_Sunshine> dragedList = new List<SunshineGame_Sunshine>();
    
    void Start()
    {
        enabled = false;
        totalTime = 120f;
        timerSize = timerMask.rect.height;
        dragBox.gameObject.SetActive(false);

        minigamePopup.onStart = () => { StartGame(); };
        minigamePopup.onPause = () => { enabled = false; };
        minigamePopup.onResume = () => { enabled = true; };
    }

    void StartGame()
    {
        currentTime = totalTime;
        score = 0;
        scoreText.text = $"{score}";
        enabled = true;
        SpawnSuns();
    }

    void Update()
    {
        // timer
        if (currentTime <= 0f) {
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

    void SpawnSuns()
    {
        float spacingX = 0.75f;
        float spacingY = 0.92f;
        while(startPoint.childCount > 0)
            Destroy(startPoint.GetChild(0).gameObject);
        for (int y = 0; y < 10; y++) {
            for (int x = 0; x < 17; x++) {
                Vector2 spawnPos = new Vector2(startPoint.position.x + (x*spacingX), startPoint.position.y - (y*spacingY));
                GameObject sunObj = Instantiate(sunPrefab, spawnPos, Quaternion.identity);
                sunObj.transform.SetParent(startPoint);
            }
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
        Camera cam = Camera.main;
        float depth = 0f;

        Vector3 w0 = cam.ScreenToWorldPoint(new Vector3(startPos.x, startPos.y, depth));
        Vector3 w1 = cam.ScreenToWorldPoint(new Vector3(endPos.x, endPos.y, depth));
        Vector2 min = new Vector2(Mathf.Min(w0.x, w1.x), Mathf.Min(w0.y, w1.y));
        Vector2 max = new Vector2(Mathf.Max(w0.x, w1.x), Mathf.Max(w0.y, w1.y));

        Collider2D[] hits = Physics2D.OverlapAreaAll(min, max);
        dragedList.Clear();
        for (int i = 0; i < hits.Length; i++) {
            SunshineGame_Sunshine sun = hits[i].GetComponent<SunshineGame_Sunshine>();
            if (sun != null)
                dragedList.Add(sun);
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

    void EndGame()
    {
        enabled = false;
        minigamePopup.RewardPopup("SunshineGame", gold: score/10*10);
    }
}
