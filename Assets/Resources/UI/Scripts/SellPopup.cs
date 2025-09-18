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
    [SerializeField] Image CountImage;
    [SerializeField] Image GoldImage;

    [Header("Done UI")]
    [SerializeField] Button doneOkBtn;
    [Header("Anim")]
    [SerializeField] float slideOffset = 80f;
    [SerializeField] float openDur = 0.20f, closeDur = 0.18f, doneCloseDur = 0.22f;

    [Header("API")]
    [SerializeField] string sellEndpoint = "/api/inventory/update";

    ObjectDatabase db;
    int index, unitPrice, max, sel = 1;
    bool selling;

    [Serializable] class InvUpdateReq { public long object_type; public int object_count; }
    [Serializable] class Envelope { public int status; public string message; public string data; }
    [Serializable] class InvGetRow { public long object_type; public int object_count; }
    [Serializable] class InvGetEnv { public int status; public string message; public InvGetRow[] data; }

    void Awake()
    {
        Debug.Log($"[SellPopup.Awake] name={name}, path={GetPath(transform)}, scene={gameObject.scene.path}, endpoint={sellEndpoint}");
    }
    string GetPath(Transform t)
    {
        var p = t.name;
        while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
        return p;
    }

    public void Open(ObjectDatabase database, int itemIndex, int defaultUnitPrice)
    {
        if (sellEndpoint != null && sellEndpoint.EndsWith("/object"))
        {
            Debug.LogWarning($"[Sell] endpoint had trailing '/object'. Fixing: {sellEndpoint}");
            sellEndpoint = sellEndpoint.Substring(0, sellEndpoint.Length - "/object".Length);
        }
        Debug.Log($"[Sell] Using endpoint: {sellEndpoint}");

        db = database;
        index = itemIndex;
        unitPrice = defaultUnitPrice;

        var name = db.GetName(index);

        if (unitPrice > 0)
        {
            itemName.text = $"{name}을(를) 판매하시겠습니까?\n(판매한 물품은 되돌릴 수 없습니다.)";

            max = Mathf.Max(0, db.GetCountFromIndex(index));
            sel = (max > 0) ? Mathf.Clamp(sel, 1, max) : 0;

            // UI 켜기
            countTxt.gameObject.SetActive(true);
            goldTxt.gameObject.SetActive(true);
            plusBtn.gameObject.SetActive(true);
            minusBtn.gameObject.SetActive(true);
            okBtn.gameObject.SetActive(true);
            CountImage.gameObject.SetActive(true);
            GoldImage.gameObject.SetActive(true);
        }
        else
        {
            itemName.text = $"{name}은(는)\n 판매할 수 없는 아이템입니다.";
            max = 0;
            sel = 0;

            // UI 숨기기
            countTxt.gameObject.SetActive(false);
            goldTxt.gameObject.SetActive(false);
            plusBtn.gameObject.SetActive(false);
            minusBtn.gameObject.SetActive(false);
            okBtn.gameObject.SetActive(true);
            CountImage.gameObject.SetActive(false);
            GoldImage.gameObject.SetActive(false);
        }

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
            StartCoroutine(SellFlow());
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

        APIManager.Instance.Put(sellEndpoint, json,
            onSuccess: ok =>
            {
                Debug.Log($"[Sell] PUT OK: {ok}");
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

    System.Collections.IEnumerator SellFlow()
    {
        // 1. 이미 판매 중이거나 판매 수량/최대치가 0 이하이면 종료
        if (selling || max <= 0 || sel <= 0) yield break;
        // 2. 판매 상태로 전환 및 UI 갱신
        selling = true; Refresh();

        // 3. 상점 아이템 번호 조회
        long storeNo = db != null ? db.GetStoreGoodsNumber(index) : -1;
        // 4. 잘못된 번호면 에러 처리 및 복구
        if (storeNo <= 0)
        {
            Debug.LogError($"[Sell] invalid storeGoodsNumber: {storeNo}");
            OnSellFailRestore();
            yield break;
        }

        // 5. 서버에서 현재 수량 조회
        int serverCount = -1;
        yield return StartCoroutine(GetServerCount(storeNo, v => serverCount = v));
        // 6. 조회 실패 시 에러 처리 및 복구
        if (serverCount < 0)
        {
            Debug.LogError("[Sell] preflight GET failed");
            OnSellFailRestore();
            yield break;
        }

        // 7. 실제 판매 수량 및 최종 수량 계산
        int sellCnt = Mathf.Min(sel, serverCount);
        int finalCount = Mathf.Max(0, serverCount - sellCnt);

        // 8. 변경 사항 없으면 종료
        if (finalCount == serverCount)
        {
            Debug.Log("[Sell] nothing to change (serverCount == finalCount)");
            selling = false; Refresh();
            yield break;
        }

        // 9. 판매 요청 페이로드 생성 및 로그
        var payload = new InvUpdateReq { object_type = storeNo, object_count = finalCount };
        string json = JsonUtility.ToJson(payload);
        Debug.Log($"[Sell] FINAL PUT {sellEndpoint} body={json} (serverBefore={serverCount}, sell={sellCnt}, final={finalCount})");

        // 10. PUT 요청 및 결과 대기
        bool postDone = false, postOk = false;
        APIManager.Instance.Put(
            sellEndpoint,
            json,
            ok => { postOk = true; Debug.Log($"[Sell] PUT OK: {ok}"); postDone = true; },
            err => { Debug.LogError($"[Sell] PUT ERR: {err}"); postDone = true; }
        );
        yield return new WaitUntil(() => postDone);
        // 11. 실패 시 복구
        if (!postOk) { OnSellFailRestore(); yield break; }

        // 12. 서버 수량 재확인
        int after = -1;
        yield return StartCoroutine(GetServerCount(storeNo, v => after = v));
        Debug.Log($"[Sell] verify serverCount={after}, expected={finalCount}");

        // 13. 정상 반영 시 보상 지급 및 UI 처리
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
        // 14. 불일치 시 에러 및 복구
        else
        {
            Debug.LogError("[Sell] verify failed: server != expected (백엔드 규칙/권한/트랜잭션 확인 필요)");
            OnSellFailRestore();
        }
    }

    // 기존 메서드 전체 교체
    System.Collections.IEnumerator GetServerCount(long storeNo, System.Action<int> onDone)
    {
        bool doneFlag = false;
        int val = 0;              // 기본값 0 (항목이 없으면 0개로 간주)
        bool hardFail = false;    // 네트워크/파싱 진짜 실패만 -1로 반환

        APIManager.Instance.Get(
            "/api/inventory",
            ok =>
            {
                try
                {
                // (선택) 원문 프리뷰 로그
                string preview = ok.Length > 400 ? ok.Substring(0, 400) + "...(truncated)" : ok;
                    Debug.Log($"[Sell] GET ok raw preview: {preview}");

                    var env = JsonUtility.FromJson<InvGetEnv>(ok);
                    if (env?.data != null && env.data.Length > 0)
                    {
                        bool found = false;
                        foreach (var r in env.data)
                        {
                            if (r.object_type == storeNo) { val = r.object_count; found = true; break; }
                        }
                        if (!found)
                        {
                        // 항목이 없으면 0으로 간주
                        val = 0;
                            Debug.Log($"[Sell] inventory missing key={storeNo}, defaulting to 0");
                        }
                    }
                    else
                    {
                    // 전체가 빈 배열이면 전부 0으로 간주
                    val = 0;
                        Debug.Log("[Sell] inventory empty list, defaulting to 0 for all");
                    }
                }
                catch (Exception e)
                {
                    hardFail = true;
                    Debug.LogError($"[Sell] GET parse error: {e}");
                }
                finally { doneFlag = true; }
            },
            err =>
            {
                hardFail = true;
                Debug.LogError($"[Sell] GET inventory error: {err}");
                doneFlag = true;
            }
        );

        yield return new WaitUntil(() => doneFlag);
        onDone?.Invoke(hardFail ? -1 : val);  // 하드 실패만 -1, 아니면 0 이상
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
