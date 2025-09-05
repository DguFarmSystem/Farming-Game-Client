using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance;

    [Header("UI 참조")]
    public CanvasGroup CollectionPanel;      // 전체 패널
    public RectTransform bookRect;           // 책 UI
    public float slideDuration = 0.5f;       // 슬라이드 시간
    public Image bookImage;                  // 배경 이미지
    public Sprite[] gradeBooks;              // 0~3 등급별 배경

    [SerializeField] private CollectionSlotManager slotManager;

    [Header("API")]
    [SerializeField] private string dexEndpoint = "/api/dex";

    private Vector2 hiddenPos;

    private void Awake()
    {
        CollectionPanel.alpha = 0f;
        CollectionPanel.blocksRaycasts = false;
        hiddenPos = new Vector2(0, -Screen.height);
        bookRect.anchoredPosition = hiddenPos;

        gameObject.SetActive(false);
    }

    public void Open()
    {
        CollectionPanel.transform.SetAsLastSibling();
        gameObject.SetActive(true);

        CollectionPanel.DOFade(1, 0.3f);
        CollectionPanel.blocksRaycasts = true;
        bookRect.DOAnchorPosY(0, slideDuration).SetEase(Ease.OutBack);

        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        yield return StartCoroutine(SyncDexFromServer()); // 실패해도 그냥 진행

        SetGrade(0);
        slotManager?.ShowSlots(0);
    }

    public void Close()
    {
        GameManager.Sound.SFXPlay("SFX_ButtonCancle");
        CollectionPanel.blocksRaycasts = false;
        bookRect.DOAnchorPos(hiddenPos, slideDuration).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                CollectionPanel.DOFade(0, 0.3f).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            });
    }

    public void SetGrade(int index)
    {
        if (index >= 0 && index < gradeBooks.Length)
            bookImage.sprite = gradeBooks[index];

        slotManager?.ShowSlots(index);
    }

    private IEnumerator SyncDexFromServer()
    {
        bool done = false;
        string body = null;
        string error = null;

        APIManager.Instance.Get(dexEndpoint,
            onSuccess: (res) => { body = res; done = true; },
            onError: (err) => { error = err; done = true; }
        );

        while (!done) yield return null;

        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogWarning("[DEX] 요청 실패: " + error);
            yield break;
        }

        try
        {
            var jo = JObject.Parse(body);
            var arr = jo["data"] as JArray;
            if (arr == null)
            {
                Debug.LogWarning("[DEX] data 배열 없음");
                yield break;
            }

            var collected = new HashSet<int>(
                arr.Select(t => (int)(t["dexId"] ?? 0))
            );

            var db = FlowerDataManager.Instance;
            if (db?.flowerData?.flowerList == null)
            {
                Debug.LogWarning("[DEX] FlowerData 비어 있음");
                yield break;
            }

            foreach (var f in db.flowerData.flowerList)
            {
                if (f == null) continue;
                f.isRegistered = collected.Contains(f.dexId);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[DEX] JSON 파싱/반영 오류: " + e);
        }
    }
}
