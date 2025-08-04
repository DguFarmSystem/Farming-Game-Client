using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SunshineGame_Manager : MonoBehaviour
{
    public Transform layout;
    public Image timerMask;
    public RectTransform dragBox;
    public GameObject sunPrefab;

    private BoxCollider2D dragCollider;
    private float timerSize;
    private Vector2 startPos;
    private Vector2 endPos;
    private List<Collider2D> dragedList = new List<Collider2D>();

    void Start()
    {
        timerSize = timerMask.rectTransform.rect.height;
        SetValue(0.4f);
        dragBox.gameObject.SetActive(false);
        dragCollider = dragBox.gameObject.GetComponent<BoxCollider2D>();

        Vector2 origin = layout.position;
        for (int y = 0; y < 10; y++) {
            for (int x = 0; x < 17; x++) {
                Vector2 spawnPos = origin + new Vector2(x * 0.748f, -y*0.9365f);
                Instantiate(sunPrefab, spawnPos, Quaternion.identity, layout);
            }
        }
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            dragBox.gameObject.SetActive(true);
            startPos = Input.mousePosition;
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, Input.mousePosition, null, out startPos);
        }
        if (Input.GetMouseButton(0)) {
            endPos = Input.mousePosition;
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, Input.mousePosition, null, out endPos);
            DrawDragBox();
        }
        if (Input.GetMouseButtonUp(0)) {
            Calculate();
            dragBox.gameObject.SetActive(false);
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
        dragedList.Clear();
        dragedList = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D(){ useTriggers = true, useLayerMask = false, useDepth = false };
        
        Vector3[] corners = new Vector3[4];
        dragBox.GetWorldCorners(corners);
        // 0 = bottom-left, 2 = top-right
        Vector3 worldCenter = (corners[0] + corners[2]) / 2f;
        Vector2 worldSize = corners[2] - corners[0];

        dragCollider.transform.position = worldCenter;
        dragCollider.size = worldSize;
        dragCollider.Overlap(filter, dragedList);
    }

    void Calculate()
    {
        Debug.Log($"calculate() : {dragedList.Count}");
        Debug.Log(dragCollider.transform.position);
        Debug.Log(dragCollider.size);

        int sum = 0;
        foreach(var c in dragedList)
            sum += c.gameObject.GetComponent<SunshineGame_Sunshine>().GetNum();
        Debug.Log($"sum: {sum}");
        if (sum != 10)
            return;
        foreach(var c in dragedList)
            c.gameObject.GetComponent<SunshineGame_Sunshine>().Pop();
    }
}
