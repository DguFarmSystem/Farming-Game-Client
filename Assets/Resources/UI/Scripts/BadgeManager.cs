using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections.Generic;

public class BadgeManager : MonoBehaviour
{
    public static BadgeManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private RectTransform caseRoot;
    [SerializeField] private RectTransform badgeLid;
    [SerializeField] private Image[] badgeSlots;         // Badge_0 ~ Badge_10
    [SerializeField] private GameObject closeButton;
    [SerializeField] private GameObject badgeSlotsParent;

    [Header("Animation Settings")]
    [SerializeField] private Vector2 caseHiddenPos = new Vector2(0, -1200f);
    [SerializeField] private Vector2 caseVisiblePos = Vector2.zero;

    [Header("Data")]
    [SerializeField] private BadgeDatabase badgeDB;

    // 뱃지 동상 
    [Header("Reward Dispatch (optional)")]
    [SerializeField] private ShopPurchaseBridge purchaseBridge;
    [SerializeField] private ObjectDatabase objectDatabase;

    // ===== Server DTOs =====
    [Serializable] private class ServerBadge { public long badgeId; public int badgeType; }
    [Serializable] private class ServerBadgeList { public ServerBadge[] items; }
    [Serializable] private class BadgePostBody { public int badgeType; }
    [Serializable] private class BadgePostResp { public long badgeId; public int badgeType; }

    [SerializeField] private GameObject TitleUIPrefab;

    private Sequence openSeq, closeSeq;
    private bool isOpening;

    private bool[] unlocked;
    private readonly HashSet<int> serverRecordedTypes = new HashSet<int>(); // 중복 POST 방지

    private const string RewardGrantedKeyPrefix = "badge_reward_granted_"; // 동상 중복 방지 

    private void Awake()
    {
        Instance = this;

        // 방어: 레퍼런스 누락 시 경고
        if (!caseRoot || !badgeLid || badgeSlots == null || badgeSlots.Length == 0 || !badgeSlotsParent || !closeButton)
        {
            Debug.LogWarning("[Badge] 레퍼런스 누락 여부 확인 필요");
        }

        // 배열 길이 안전 보정
        int len = badgeSlots != null ? badgeSlots.Length : 0;
        unlocked = new bool[len];

        // 슬롯 초기화
        if (badgeSlots != null)
        {
            for (int i = 0; i < badgeSlots.Length; i++)
            {
                badgeSlots[i].sprite = null;
                var c = badgeSlots[i].color;
                c.a = 0f;
                badgeSlots[i].color = c;
            }
        }

        InitBadges();

        // 초기 위치/상태 세팅
        if (caseRoot) caseRoot.anchoredPosition = caseHiddenPos;
        if (badgeLid)
        {
            badgeLid.anchoredPosition = caseHiddenPos;
            badgeLid.localScale = Vector3.one;
            badgeLid.gameObject.SetActive(false);
        }
        if (badgeSlotsParent) badgeSlotsParent.SetActive(false);

        // 시작 시 비활성 (요청 반영)
        gameObject.SetActive(false);
    }

    private void InitBadges()
    {
        if (badgeSlots == null || badgeDB == null) return;

        for (int i = 0; i < badgeSlots.Length; i++)
        {
            badgeSlots[i].sprite = null;
            badgeSlots[i].color = new Color(1, 1, 1, 0f); // 아예 안 보이게

            var trigger = badgeSlots[i].GetComponent<BadgeTrigger>();
            if (trigger != null && i < badgeDB.items.Count)
            {
                trigger.data = badgeDB.items[i];
            }
        }
    }

    private ServerBadge[] ParseServerBadges(string json)
    {
        if (string.IsNullOrEmpty(json)) return Array.Empty<ServerBadge>();

        string wrapped = "{\"items\":" + json + "}";
        var list = JsonUtility.FromJson<ServerBadgeList>(wrapped);
        return list?.items ?? Array.Empty<ServerBadge>();
    }

    public void SyncBadgesFromServerAndShow()
    {
        if (APIManager.Instance == null)
        {
            Debug.LogError("[Badge] APIManager.Instance 가 null");
            return;
        }

        APIManager.Instance.Get("/api/badge",
            ok =>
            {
                var arr = ParseServerBadges(ok);

                if (unlocked == null || unlocked.Length != badgeSlots.Length)
                    unlocked = new bool[badgeSlots.Length];

                foreach (var b in arr)
                {
                    int idx = b.badgeType;
                    if (idx >= 0 && idx < unlocked.Length)
                    {
                        unlocked[idx] = true;
                        serverRecordedTypes.Add(idx); 
                    }
                }

                RefreshBadges();
                Debug.Log($"[Badge] 서버 로드 완료: {arr.Length}개 획득 표시");

                ReevaluateBadges();
            },
            err => { Debug.LogError("[Badge] 서버 로드 실패: " + err); }
        );
    }

    public void PostBadgeToServer(int badgeType)
    {
        if (APIManager.Instance == null)
        {
            Debug.LogError("[Badge] APIManager1.Instance 가 null");
            return;
        }
        if (badgeSlots == null || badgeType < 0 || badgeType >= badgeSlots.Length)
        {
            Debug.LogWarning($"[Badge] 범위를 벗어난 badgeType={badgeType}");
            return;
        }
        if (serverRecordedTypes.Contains(badgeType))
        {
            Debug.Log($"[Badge] 이미 서버에 기록된 타입: {badgeType} (중복 POST 방지)");
            return;
        }

        var body = new BadgePostBody { badgeType = badgeType };
        string json = JsonUtility.ToJson(body);

        APIManager.Instance.Post("/api/badge", json,
            ok =>
            {
                try
                {
                    var resp = JsonUtility.FromJson<BadgePostResp>(ok);
                    serverRecordedTypes.Add(badgeType);
                    Debug.Log($"[Badge] 서버 기록 성공: type={resp?.badgeType ?? badgeType}, id={resp?.badgeId}");
                }
                catch
                {
                    serverRecordedTypes.Add(badgeType);
                    Debug.Log($"[Badge] 서버 기록 성공(파싱 생략): type={badgeType}, raw={ok}");
                }
            },
            err =>
            {
                Debug.LogError($"[Badge] 서버 기록 실패(type={badgeType}): {err}");
            }
        );
    }

    public void UnlockBadge(int index)
    {
        if (unlocked == null || index < 0 || index >= unlocked.Length) return;

        if (!unlocked[index])
        {
            unlocked[index] = true;
            RefreshBadges();
            PostBadgeToServer(index);

            TryGrantBadgeReward(index); // 뱃지 해금 보상 동상 지급 
        }
    }

    public void Open()
    {
        if (isOpening) return;
        isOpening = true;

        ReevaluateBadges();
        SyncBadgesFromServerAndShow();

        openSeq?.Kill(); closeSeq?.Kill();
        caseRoot?.DOKill(); badgeLid?.DOKill();

        if (badgeLid)
        {
            badgeLid.transform.SetAsLastSibling();
            int lidIdx = badgeLid.transform.GetSiblingIndex();
            if (badgeSlotsParent) badgeSlotsParent.transform.SetSiblingIndex(Mathf.Max(0, lidIdx - 1));
        }

        gameObject.SetActive(true);
        closeButton?.SetActive(true);

        if (caseRoot) caseRoot.anchoredPosition = caseHiddenPos;
        if (badgeLid)
        {
            badgeLid.anchoredPosition = caseHiddenPos;
            badgeLid.localScale = Vector3.one;
            badgeLid.gameObject.SetActive(true);
        }
        if (badgeSlotsParent) badgeSlotsParent.SetActive(false);

        openSeq = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        if (caseRoot)
        {
            openSeq.Append(
                caseRoot.DOAnchorPos(caseVisiblePos, 0.6f)
                        .SetEase(Ease.OutQuad)
                        .SetLink(caseRoot.gameObject, LinkBehaviour.KillOnDestroy)
            );
        }
        if (badgeLid)
        {
            openSeq.Join(
                badgeLid.DOAnchorPos(caseVisiblePos, 0.6f)
                        .SetEase(Ease.OutQuad)
                        .SetLink(badgeLid.gameObject, LinkBehaviour.KillOnDestroy)
            );

            openSeq.AppendInterval(0.3f);
            openSeq.Append(
                badgeLid.DOScale(1.15f, 0.5f)
                        .SetEase(Ease.OutSine)
                        .SetLink(badgeLid.gameObject, LinkBehaviour.KillOnDestroy)
            );

            openSeq.AppendCallback(() =>
            {
                if (!this || !badgeLid || !badgeSlotsParent) return;
                badgeSlotsParent.SetActive(true);

                badgeLid.transform.SetAsLastSibling();
                int lidIdx = badgeLid.transform.GetSiblingIndex();
                badgeSlotsParent.transform.SetSiblingIndex(Mathf.Max(0, lidIdx - 1));
            });

            openSeq.Append(
                badgeLid.DOAnchorPosY(caseVisiblePos.y + 1000f, 1.5f)
                        .SetEase(Ease.InOutSine)
                        .SetLink(badgeLid.gameObject, LinkBehaviour.KillOnDestroy)
            );
            openSeq.AppendCallback(() => badgeLid.gameObject.SetActive(false));
        }

        openSeq.OnComplete(() => { isOpening = false; });
    }

    public void Close()
    {
        try { GameManager.Sound.SFXPlay("SFX_ButtonCancle"); } catch { }

        openSeq?.Kill(); closeSeq?.Kill();
        caseRoot?.DOKill(); badgeLid?.DOKill();

        closeButton?.SetActive(false);

        closeSeq = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        if (caseRoot)
        {
            closeSeq.Append(
                caseRoot.DOAnchorPos(caseHiddenPos, 0.5f)
                        .SetEase(Ease.InBack)
                        .SetLink(caseRoot.gameObject, LinkBehaviour.KillOnDestroy)
            );
        }

        closeSeq.OnComplete(() => gameObject.SetActive(false));
    }

    private void OnDisable()
    {
        openSeq?.Kill();
        closeSeq?.Kill();
        caseRoot?.DOKill();
        badgeLid?.DOKill();
        isOpening = false;
    }

    private void OnDestroy()
    {
        openSeq?.Kill();
        closeSeq?.Kill();
        caseRoot?.DOKill();
        badgeLid?.DOKill();
    }

    public void ReevaluateBadges()
    {
        var fdm = FlowerDataManager.Instance;
        if (fdm == null || fdm.flowerData == null || badgeDB == null)
        {
            Debug.LogWarning("[Badge] 데이터 없음");
            return;
        }

        var list = fdm.flowerData.flowerList.Where(f => f != null).ToList();
        int totalReg = list.Count(f => f.isRegistered);
        int shinyReg = list.Count(f => f.isRegistered && IsShiny(f.flowerName));
        int CountAll(FlowerRarity r) => list.Count(f => f.rarity == r);
        int CountReg(FlowerRarity r) => list.Count(f => f.rarity == r && f.isRegistered);

        Debug.Log($"[Badge] totalReg={totalReg}, shinyReg={shinyReg}");

        int len = Mathf.Min(badgeSlots?.Length ?? 0, badgeDB.items.Count);
        if (unlocked == null || unlocked.Length != len) unlocked = new bool[len];

        bool changed = false;

        for (int i = 0; i < len; i++)
        {
            var bd = badgeDB.items[i];
            if (bd == null) continue;

            bool ok = false;
            string reason = "";

            switch (bd.type)
            {
                case BadgeType.TotalRegisteredAtLeast:
                    ok = (totalReg >= bd.threshold);
                    reason = $"totalReg({totalReg}) >= threshold({bd.threshold})";
                    break;

                case BadgeType.ShinyRegisteredAtLeast:
                    ok = (shinyReg >= bd.threshold);
                    reason = $"shinyReg({shinyReg}) >= threshold({bd.threshold})";
                    break;

                case BadgeType.RarityAllCollected:
                    int all = CountAll(bd.rarity);
                    int reg = CountReg(bd.rarity);
                    ok = (all > 0) && (reg == all);
                    reason = $"rarity={bd.rarity}, collected {reg}/{all}";
                    break;

                case BadgeType.WhitelistAllCollected:
                    int need = bd.nameWhitelist?.Count ?? 0;
                    int have = bd.nameWhitelist?.Count(n => list.Any(f => f.flowerName == n && f.isRegistered)) ?? 0;
                    ok = need > 0 && have == need;
                    reason = $"whitelist {have}/{need} matched";
                    break;

                    // 정원 추가 
            }

            if (ok && !unlocked[i])
            {
                Debug.Log($"뱃지 해금 -> idx={i}, title={bd.title}, type={bd.type}, reason=({reason})");

                unlocked[i] = true;
                changed = true;

                PostBadgeToServer(i);

                var data = badgeDB.items[i];
                if (data != null && UIManager.Instance != null)
                    UIManager.Instance.EnqueueBadgeUnlock(data.icon, data.title, data.description);

                TryGrantBadgeReward(i);  // 동상 지급 
            }
            else
            {
                Debug.Log($"[Badge] idx={i}, title={bd.title}, ok={ok}, unlocked={unlocked[i]}, reason=({reason})");
            }
        }

        if (changed) RefreshBadges();
    }

    static bool IsShiny(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        return name.IndexOf("Shiny", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void RefreshBadges()
    {
        if (badgeDB == null || badgeSlots == null) return;

        int len = Mathf.Min(badgeSlots.Length, badgeDB.items.Count);
        for (int i = 0; i < len; i++)
        {
            var data = badgeDB.items[i];
            if (unlocked != null && i < unlocked.Length && unlocked[i])
            {
                badgeSlots[i].sprite = data.icon;

                var c = badgeSlots[i].color;
                c.a = 1f;
                badgeSlots[i].color = c;
                badgeSlots[i].preserveAspect = true;
            }
            else
            {
                badgeSlots[i].sprite = null;
                var c = badgeSlots[i].color;
                c.a = 0f;
                badgeSlots[i].color = c;
            }
        }
    }

    private void TryGrantBadgeReward(int badgeIndex)
    {
        if (badgeDB == null || badgeIndex < 0 || badgeIndex >= badgeDB.items.Count) return;
        var bd = badgeDB.items[badgeIndex];
        if (bd == null) return;

        long storeNo = bd.rewardStoreGoodsNumber;
        int qty = Mathf.Max(1, bd.rewardCount);
        if (storeNo <= 0 || qty <= 0) return;

        string guardKey = RewardGrantedKeyPrefix + badgeIndex;
        if (PlayerPrefs.GetInt(guardKey, 0) == 1)
        {
            Debug.Log($"[Badge] reward already granted (idx={badgeIndex})");
            return;
        }

        var bridge = purchaseBridge != null
            ? purchaseBridge
            : FindFirstObjectByType<ShopPurchaseBridge>(FindObjectsInactive.Include);

        if (bridge != null)
        {
            bridge.OnPurchased(storeNo, bd.title, qty);
            Debug.Log($"[Badge] reward dispatched via ShopPurchaseBridge: {storeNo} x{qty}");
        }
        else
        {
            Debug.LogWarning("[Badge] ShopPurchaseBridge not found, reward skipped");
            return;
        }

        PlayerPrefs.SetInt(guardKey, 1);
        PlayerPrefs.Save();
    }

}
