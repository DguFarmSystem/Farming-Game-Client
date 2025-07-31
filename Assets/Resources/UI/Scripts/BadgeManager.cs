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

    private bool[] badgeUnlocked = new bool[11];

    private void Start()
    {
        InitBadges();
        UnlockBadge(0);
        UnlockBadge(8);
        UnlockBadge(6);
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
        gameObject.SetActive(true);
        RefreshBadges();
        closeButton.SetActive(true);

        Vector2 startPos = caseHiddenPos;
        Vector2 targetPos = caseVisiblePos;

        caseRoot.anchoredPosition = startPos;
        badgeLid.anchoredPosition = startPos;
        badgeLid.localScale = Vector3.one;
        badgeLid.gameObject.SetActive(true);
        badgeSlotsParent.SetActive(false); // 초기에는 숨김

        Sequence openSeq = DOTween.Sequence();

        // 1. 케이스+뚜껑 슬라이드 업 (0.6초)
        openSeq.Append(caseRoot.DOAnchorPos(targetPos, 0.6f).SetEase(Ease.OutQuad));
        openSeq.Join(badgeLid.DOAnchorPos(targetPos, 0.6f).SetEase(Ease.OutQuad));

        // 2. 잠깐 대기 (0.3초)
        openSeq.AppendInterval(0.3f);

        // 3. 뚜껑 커짐 (0.3초) → 뱃지 보드 사이즈 맞춤
        openSeq.Append(badgeLid.DOScale(1.15f, 0.5f).SetEase(Ease.OutSine));

        // 4. 커진 직후 뱃지 보드 등장
        openSeq.AppendCallback(() => badgeSlotsParent.SetActive(true));

        // 5. 뚜껑 위로 슬라이드 (0.8초)
        openSeq.Append(badgeLid.DOAnchorPosY(targetPos.y + 1000f, 1.5f).SetEase(Ease.InOutSine));

        // 6. 뚜껑 비활성화
        openSeq.AppendCallback(() => badgeLid.gameObject.SetActive(false));
    }

    public void Close()
    {
        closeButton.SetActive(false);

        // 내부 전체 UI를 슬라이드 다운
        Sequence closeSeq = DOTween.Sequence();
        closeSeq.Append(caseRoot.DOAnchorPos(caseHiddenPos, 0.5f).SetEase(Ease.InBack));
        closeSeq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
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
                badgeSlots[i].color = new Color(1, 1, 1, 0f); // 안 보이게
            }
        }
    }
}
