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
    [SerializeField] private GameObject closeButton;

    [Header("Animation Settings")]
    [SerializeField] private Vector2 caseHiddenPos = new Vector2(0, -1200f);
    [SerializeField] private Vector2 caseVisiblePos = Vector2.zero;

    private bool[] badgeUnlocked = new bool[11];

    private void Start()
    {
        InitBadges();
        UnlockBadge(0);
        UnlockBadge(8);
    }

    private void InitBadges()
    {
        for (int i = 0; i < badgeSlots.Length; i++)
        {
            badgeSlots[i].sprite = null;
            badgeSlots[i].color = new Color(1, 1, 1, 0f); // 아예 안 보이게
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

        // 초기 위치와 상태 설정
        caseRoot.anchoredPosition = caseHiddenPos;
        badgeLid.anchoredPosition = caseHiddenPos;
        badgeLid.localScale = Vector3.one;
        badgeLid.gameObject.SetActive(true);

        Sequence openSeq = DOTween.Sequence();

        // 1. 케이스와 뚜껑 함께 올라오기 (0.5초)
        openSeq.Append(caseRoot.DOAnchorPos(caseVisiblePos, 0.5f).SetEase(Ease.OutBack));
        openSeq.Join(badgeLid.DOAnchorPos(caseVisiblePos, 0.5f).SetEase(Ease.OutBack));

        // 2. 뚜껑 커지기 (0.3초)
        openSeq.Append(badgeLid.DOScale(1.2f, 0.3f).SetEase(Ease.OutQuad));

        // 3. 뚜껑 위로 사라지기 (1초)
        openSeq.Append(badgeLid.DOAnchorPosY(600f, 1f).SetEase(Ease.InOutQuad));

        // 4. 뚜껑 비활성화
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
