using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class BadgeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform caseRoot;
    [SerializeField] private RectTransform badgeLid;
    [SerializeField] private Image[] badgeSlots;         // Badge_0 ~ Badge_10
    [SerializeField] private Sprite[] badgeSprites;      // 획득한 뱃지 Sprite
    [SerializeField] private BadgeData[] badgeDatas;
    [SerializeField] private GameObject closeButton;
    [SerializeField] private GameObject badgeSlotsParent;

    [Header("Animation Settings")]
    [SerializeField] private Vector2 caseHiddenPos = new Vector2(0, -1200f);
    [SerializeField] private Vector2 caseVisiblePos = Vector2.zero;

    private Sequence openSeq, closeSeq;
    private bool isOpening;

    private bool[] badgeUnlocked = new bool[11];

    private void Awake()
    {
        InitBadges();
        UnlockBadge(0); UnlockBadge(8); UnlockBadge(6);

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
                trigger.data = badgeDatas[i];  // 연결
            }
        }
    }

    public void UnlockBadge(int index)
    {
        if (index < 0 || index >= badgeUnlocked.Length) return;

        badgeUnlocked[index] = true;
    }

    public void Open()
    {
        if (isOpening) return;
        isOpening = true;

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

    private void RefreshBadges()
    {
        for (int i = 0; i < badgeSlots.Length; i++)
        {
            if (badgeUnlocked[i])
            {
                badgeSlots[i].sprite = badgeSprites[i];
                badgeSlots[i].color = Color.white;
            }
            else
            {
                badgeSlots[i].sprite = null;
                badgeSlots[i].color = new Color(1, 1, 1, 0f);
            }
        }
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

}
