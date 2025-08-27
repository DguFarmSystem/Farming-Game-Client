using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class SellPopup : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] RectTransform confirm;
    [SerializeField] RectTransform done;
    [Header("Confirm UI")]
    [SerializeField] TMP_Text itemName;
    [SerializeField] TMP_Text countTxt;
    [SerializeField] TMP_Text goldTxt;
    [SerializeField] Button plusBtn, minusBtn, okBtn, cancelBtn;
    [Header("Done UI")]
    [SerializeField] Button doneOkBtn;
    [Header("Anim")]
    [SerializeField] float slideOffset = 80f;
    [SerializeField] float openDur = 0.20f, closeDur = 0.18f, doneCloseDur = 0.22f;

    [Header("API")]
    [SerializeField] string sellEndpoint = "/api/inventory/update/object";

    ObjectDatabase db;
    int index, unitPrice, max, sel = 1;
    bool selling;

    [Serializable] class InvUpdateReq { public long object_type; public int object_count; }
    [Serializable] class Envelope { public int status; public string message; public string data; }
    [Serializable] class InvGetRow { public long object_type; public int object_count; }
    [Serializable] class InvGetEnv { public int status; public string message; public InvGetRow[] data; }

    public void Open(ObjectDatabase database, int itemIndex, int defaultUnitPrice)
    {
        db = database;
        index = itemIndex;
        unitPrice = defaultUnitPrice;

        var name = db.GetName(index);
        itemName.text = $"{name} 판매하시겠습니까?\n(판매한 물품은 되돌릴 수 없습니다.)";

        max = Mathf.Max(0, db.GetCountFromIndex(index));
        sel = (max > 0) ? Mathf.Clamp(sel, 1, max) : 0;

        Wire();
        Refresh();

        done.gameObject.SetActive(false);
        confirm.gameObject.SetActive(true);
        SlideFromBelow(confirm, openDur);
        gameObject.SetActive(true);
    }

    void Wire()
    {
        plusBtn.onClick.RemoveAllListeners();
        minusBtn.onClick.RemoveAllListeners();
        okBtn.onClick.RemoveAllListeners();
        cancelBtn.onClick.RemoveAllListeners();
        doneOkBtn.onClick.RemoveAllListeners();

        plusBtn.onClick.AddListener(() => { if (sel < max) { sel++; Refresh(); } });
        minusBtn.onClick.AddListener(() => { if (sel > 1) { sel--; Refresh(); } });

        // 취소: 슬라이드다운 + SFX_ButtonCancle (SlideToBelow 내부)
        cancelBtn.onClick.AddListener(() => SlideToBelow(confirm, closeDur, () => Destroy(gameObject)));

        // 확인(OK): 클릭 사운드 + 서버 플로우
        okBtn.onClick.AddListener(() =>
        {
            GameManager.Sound.SFXPlay("SFX_ButtonClick");
            StartCoroutine(SellFlow_FinalOnly());
        });

        // 완료 OK: 클릭 사운드 + 닫기
        doneOkBtn.onClick.AddListener(CloseDone);
    }

    void Refresh()
    {
        countTxt.text = $"{sel}/{max}";
        goldTxt.text = ((long)sel * unitPrice).ToString("N0");
        minusBtn.interactable = sel > 1;
        plusBtn.interactable = sel < max;
        okBtn.interactable = max > 0 && sel > 0 && !selling;
    }

    // (레거시 경로) 로컬 감소 후 POST하는 경로도 클릭 사운드 통일
    void SellAndShowDone()
    {
        if (selling || max <= 0 || sel <= 0) return;
        GameManager.Sound.SFXPlay("SFX_ButtonClick");

        int sellCnt = Mathf.Min(sel, max);
        int newCount = Mathf.Max(0, max - sellCnt);

        long storeNo = db != null ? db.GetStoreGoodsNumber(index) : -1;
        if (storeNo <= 0)
        {
            Debug.LogError($"[Sell] 잘못된 storeGoodsNumber: {storeNo} (index={index})");
            return;
        }

        selling = true;
        Refresh();

        var payload = new InvUpdateReq { object_type = storeNo, object_count = newCount };
        string json = JsonUtility.ToJson(payload);

        APIManager.Instance.Post(sellEndpoint, json,
            onSuccess: ok =>
            {
                Debug.Log($"[Sell] POST OK: {ok}");
                try
                {
                    var env = JsonUtility.FromJson<Envelope>(ok);
                    if (env != null && env.status >= 400)
                    {
                        Debug.LogError($"[Sell] 서버 오류 status={env.status}, msg={env.message}");
                        OnSellFailRestore();
                        return;
                    }
                }
                catch { /* 비래핑 응답이면 무시 */ }

                CurrencyManager.Instance?.AddGold(sellCnt * unitPrice);

                confirm.gameObject.SetActive(false);
                done.gameObject.SetActive(true);
                done.anchoredPosition = Vector2.zero;
                done.SetAsLastSibling();

                BagManager.Instance?.FetchInventoryAndRebuild();

                selling = false;
                Refresh();
            },
            onError: err =>
            {
                Debug.LogError($"[Sell] 판매 실패: {err}");
                OnSellFailRestore();
            }
        );
    }

    System.Collections.IEnumerator SellFlow_FinalOnly()
    {
        if (selling || max <= 0 || sel <= 0) yield break;
        selling = true; Refresh();

        long storeNo = db != null ? db.GetStoreGoodsNumber(index) : -1;
        if (storeNo <= 0)
        {
            Debug.LogError($"[Sell] invalid storeGoodsNumber: {storeNo}");
            OnSellFailRestore();
            yield break;
        }

        int serverCount = -1;
        yield return StartCoroutine(GetServerCount(storeNo, v => serverCount = v));
        if (serverCount < 0)
        {
            Debug.LogError("[Sell] preflight GET failed");
            OnSellFailRestore();
            yield break;
        }

        int sellCnt = Mathf.Min(sel, serverCount);
        int finalCount = Mathf.Max(0, serverCount - sellCnt);

        if (finalCount == serverCount)
        {
            Debug.Log("[Sell] nothing to change (serverCount == finalCount)");
            selling = false; Refresh();
            yield break;
        }

        var payload = new InvUpdateReq { object_type = storeNo, object_count = finalCount };
        string json = JsonUtility.ToJson(payload);
        Debug.Log($"[Sell] FINAL POST {sellEndpoint} body={json} (serverBefore={serverCount}, sell={sellCnt}, final={finalCount})");

        bool postDone = false, postOk = false;
        APIManager.Instance.Post(
            sellEndpoint,
            json,
            ok => { postOk = true; Debug.Log($"[Sell] POST OK: {ok}"); postDone = true; },
            err => { Debug.LogError($"[Sell] POST ERR: {err}"); postDone = true; }
        );
        yield return new WaitUntil(() => postDone);
        if (!postOk) { OnSellFailRestore(); yield break; }

        int after = -1;
        yield return StartCoroutine(GetServerCount(storeNo, v => after = v));
        Debug.Log($"[Sell] verify serverCount={after}, expected={finalCount}");

        if (after == finalCount)
        {
            CurrencyManager.Instance?.AddGold(sellCnt * unitPrice);

            confirm.gameObject.SetActive(false);
            done.gameObject.SetActive(true);
            done.anchoredPosition = Vector2.zero;
            done.SetAsLastSibling();

            BagManager.Instance?.FetchInventoryAndRebuild();

            selling = false; Refresh();
        }
        else
        {
            Debug.LogError("[Sell] verify failed: server != expected (백엔드 규칙/권한/트랜잭션 확인 필요)");
            OnSellFailRestore();
        }
    }

    System.Collections.IEnumerator GetServerCount(long storeNo, System.Action<int> onDone)
    {
        bool doneFlag = false;
        int val = -1;

        APIManager.Instance.Get(
            "/api/inventory",
            ok =>
            {
                try
                {
                    var env = JsonUtility.FromJson<InvGetEnv>(ok);
                    if (env?.data != null)
                    {
                        foreach (var r in env.data)
                        {
                            if (r.object_type == storeNo) { val = r.object_count; break; }
                        }
                    }
                }
                catch { }
                doneFlag = true;
            },
            err => { Debug.LogError("[Sell] GET inventory error: " + err); doneFlag = true; }
        );

        yield return new WaitUntil(() => doneFlag);
        onDone?.Invoke(val);
    }

    void OnSellFailRestore()
    {
        selling = false;
        Refresh();
    }

    void CloseDone()
    {
        if (done == null) return;

        // 완료 확인 버튼 클릭 사운드
        GameManager.Sound.SFXPlay("SFX_ButtonClick");

        done.DOKill();

        var parentRT = done.parent as RectTransform;
        float distance = ((parentRT != null) ? parentRT.rect.height : Screen.height) + done.rect.height;
        Vector2 target = done.anchoredPosition + Vector2.down * distance;

        done.DOAnchorPos(target, doneCloseDur).SetEase(Ease.InQuad)
            .OnComplete(() => { BagManager.Instance?.FetchInventoryAndRebuild(); Destroy(gameObject); });
    }

    void SlideFromBelow(RectTransform p, float dur)
    {
        p.DOKill();
        var start = p.anchoredPosition;
        p.anchoredPosition = start + Vector2.down * slideOffset;
        p.DOAnchorPos(start, dur).SetEase(Ease.OutQuad);
    }

    void SlideToBelow(RectTransform p, float dur, System.Action onEnd)
    {
        // 취소/닫기 사운드
        GameManager.Sound.SFXPlay("SFX_ButtonCancle");

        p.DOKill();
        var target = p.anchoredPosition + Vector2.down * slideOffset;
        p.DOAnchorPos(target, dur).SetEase(Ease.InQuad).OnComplete(() => onEnd?.Invoke());
    }
}
