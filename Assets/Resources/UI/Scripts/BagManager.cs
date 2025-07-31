using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class BagManager : MonoBehaviour
{
    public static BagManager Instance;

    public CanvasGroup panelGroup;
    public RectTransform windowRect;
    public float slideDuration = 0.5f;

    private Vector2 hiddenPos;

    void Awake()
    {
        Instance = this;
        panelGroup.alpha = 0f;
        panelGroup.blocksRaycasts = false;
        hiddenPos = new Vector2(0, -Screen.height);
        windowRect.anchoredPosition = hiddenPos;
        gameObject.SetActive(false);
    }

    public void Open()
    {
        Debug.Log("BagManager.Open 호출됨");

        gameObject.SetActive(true);

        BagSlotManager.Instance.ShowSlots();

        panelGroup.DOFade(1, 0.3f);
        panelGroup.blocksRaycasts = true;
        windowRect.DOAnchorPosY(0, slideDuration).SetEase(Ease.OutBack);
    }

    public void Close()
    {
        panelGroup.blocksRaycasts = false;
        windowRect.DOAnchorPos(hiddenPos, slideDuration).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                panelGroup.DOFade(0, 0.3f).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            });
    }
}
