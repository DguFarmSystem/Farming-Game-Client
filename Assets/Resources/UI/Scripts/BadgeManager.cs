using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;

public class BadgeManager : MonoBehaviour
{
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

    private Sequence openSeq, closeSeq;
    private bool isOpening;

    private bool[] unlocked;

    private void Awake()
    {
        unlocked = new bool[badgeSlots.Length];
        for (int i = 0; i < badgeSlots.Length; i++)
        {
            badgeSlots[i].sprite = null;
            var c = badgeSlots[i].color;
            c.a = 0f;
            badgeSlots[i].color = c;
        }

        InitBadges();

        caseRoot.anchoredPosition = caseHiddenPos;
        badgeLid.anchoredPosition = caseHiddenPos;
        badgeLid.localScale = Vector3.one;
        badgeLid.gameObject.SetActive(false);
        badgeSlotsParent.SetActive(false);

        gameObject.SetActive(false);
    }

    private void InitBadges()
    {
        for (int i = 0; i < badgeSlots.Length; i++)
        {
            badgeSlots[i].sprite = null;
            badgeSlots[i].color = new Color(1, 1, 1, 0f); // 아예 안 보이게

            var trigger = badgeSlots[i].GetComponent<BadgeTrigger>();
            if (trigger != null)
            {
                trigger.data = badgeDB.items[i];
            }
        }
    }

    public void UnlockBadge(int index)
    {
        if (index < 0 || index >= unlocked.Length) return;
        if (!unlocked[index])
        {
            unlocked[index] = true;
            RefreshBadges();
        }
    }

    public void Open()
    {
        if (isOpening) return;
        isOpening = true;

        ReevaluateBadges();

        openSeq?.Kill(); closeSeq?.Kill();
        caseRoot.DOKill(); badgeLid.DOKill();

        badgeLid.transform.SetAsLastSibling();
        int lidIdx = badgeLid.transform.GetSiblingIndex();
        badgeSlotsParent.transform.SetSiblingIndex(Mathf.Max(0, lidIdx - 1));

        gameObject.SetActive(true);
        RefreshBadges();
        closeButton.SetActive(true);

        caseRoot.anchoredPosition = caseHiddenPos;
        badgeLid.anchoredPosition = caseHiddenPos;
        badgeLid.localScale = Vector3.one;
        badgeLid.gameObject.SetActive(true);
        badgeSlotsParent.SetActive(false);

        openSeq = DOTween.Sequence()
                     .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        openSeq.Append(
            caseRoot.DOAnchorPos(caseVisiblePos, 0.6f)
                    .SetEase(Ease.OutQuad)
                    .SetLink(caseRoot.gameObject, LinkBehaviour.KillOnDestroy)
        );
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

        openSeq.OnComplete(() => { isOpening = false; });
    }

    public void Close()
    {
        openSeq?.Kill(); closeSeq?.Kill();
        caseRoot.DOKill(); badgeLid.DOKill();

        closeButton.SetActive(false);

        closeSeq = DOTween.Sequence()
                      .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        closeSeq.Append(
            caseRoot.DOAnchorPos(caseHiddenPos, 0.5f)
                    .SetEase(Ease.InBack)
                    .SetLink(caseRoot.gameObject, LinkBehaviour.KillOnDestroy)
        );
        closeSeq.OnComplete(() => gameObject.SetActive(false));
    }

    private void OnDisable()
    {
        openSeq?.Kill();
        closeSeq?.Kill();
        caseRoot?.DOKill();
        badgeLid?.DOKill();
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
        if (fdm == null || fdm.flowerData == null || badgeDB == null) { Debug.LogWarning("[Badge] 데이터 없음"); return; }

        var list = fdm.flowerData.flowerList.Where(f => f != null).ToList();
        int totalReg = list.Count(f => f.isRegistered);
        int shinyReg = list.Count(f => f.isRegistered && IsShiny(f.flowerName));
        int CountAll(FlowerRarity r) => list.Count(f => f.rarity == r);
        int CountReg(FlowerRarity r) => list.Count(f => f.rarity == r && f.isRegistered);
        // 정원 추가 

        Debug.Log($"[Badge] totalReg={totalReg}, shinyReg={shinyReg}");

        int len = Mathf.Min(badgeSlots.Length, badgeDB.items.Count);
        if (unlocked == null || unlocked.Length != len) unlocked = new bool[len];

        for (int i = 0; i < len; i++)
        {
            var bd = badgeDB.items[i];
            if (bd == null) continue;
            bool ok = false;

            switch (bd.type)
            {
                case BadgeType.TotalRegisteredAtLeast:
                    ok = (totalReg >= bd.threshold);
                    break;
                case BadgeType.ShinyRegisteredAtLeast:
                    ok = (shinyReg >= bd.threshold);
                    break;
                case BadgeType.RarityAllCollected:
                    ok = (CountAll(bd.rarity) > 0) && (CountReg(bd.rarity) == CountAll(bd.rarity));
                    break;
                case BadgeType.WhitelistAllCollected:
                    ok = bd.nameWhitelist != null && bd.nameWhitelist.Count > 0
                         && bd.nameWhitelist.All(n => list.Any(f => f.flowerName == n && f.isRegistered));
                    break;
                // 정원 추가 
            }

            Debug.Log($"[Badge] idx={i}, title={bd.title}, ok={ok}");

            if (ok && !unlocked[i]) unlocked[i] = true;
        }

        RefreshBadges();
    }


    static bool IsShiny(string name)
    => !string.IsNullOrEmpty(name) && name.Contains("이로치");

    private void RefreshBadges()
    {
        if (badgeDB == null) return;
        int len = Mathf.Min(badgeSlots.Length, badgeDB.items.Count);

        for (int i = 0; i < len; i++)
        {
            var data = badgeDB.items[i];
            if (unlocked[i])
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

}