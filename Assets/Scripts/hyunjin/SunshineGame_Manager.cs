using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SunshineGame_Manager : MonoBehaviour
{
    public Transform layout;
    public Image timerMask;
    public RectTransform dragBox;
    public TMP_Text scoreText;
    public Button tempExitButton;

    private int score;
    private float timerSize;
    private Vector2 startPos;
    private Vector2 endPos;
    private List<SunshineGame_Sunshine> dragedList = new List<SunshineGame_Sunshine>();
    
    void Start()
    {
        score = 0;
        timerSize = timerMask.rectTransform.rect.height;
        SetValue(0.4f);
        dragBox.gameObject.SetActive(false);
        tempExitButton.onClick.AddListener(EndGame);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            dragBox.gameObject.SetActive(true);
            startPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0)) {
            endPos = Input.mousePosition;
            DrawDragBox();
            SelectObjects();
        }
        if (Input.GetMouseButtonUp(0)) {
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
        SelectObjects();
    }

    public void SetValue(float value)
    {
        // timerMask.sizeDelta = new Vector2(timerMask.sizeDelta.x, timerSize*value);
        timerMask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, timerSize * value);
    }

    void SelectObjects()
    {
        Rect dragRect = GetWorldRect(dragBox);
        dragedList.Clear();

        foreach (Transform child in layout)
        {
            if (!child.gameObject.activeSelf) continue;

            SunshineGame_Sunshine sun = child.GetComponent<SunshineGame_Sunshine>();
            if (sun == null) continue;

            RectTransform sunRect = child.GetComponent<RectTransform>();
            Vector3[] corners = new Vector3[4];
            sunRect.GetWorldCorners(corners);
            Vector2 center = (corners[0] + corners[2]) / 2f;

            if (dragRect.Contains(center))
            {
                dragedList.Add(sun);
            }
        }
    }

    void Calculate()
    {
        int sum = 0;
        foreach (var sun in dragedList) {
            Debug.Log($"draged sun : {sun.GetNum()}");
            sum += sun.GetNum();

        }
        Debug.Log($"sum : {sum}");
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
        SceneLoader.Instance.ExitMiniGame();
    }
}
