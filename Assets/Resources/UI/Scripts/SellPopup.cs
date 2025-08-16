using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

    ObjectDatabase db;
    int index, unitPrice, max, sel = 1;
    bool selling;

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
        cancelBtn.onClick.AddListener(() => SlideToBelow(confirm, closeDur, () => Destroy(gameObject)));
        okBtn.onClick.AddListener(SellAndShowDone);
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

    void SellAndShowDone()
    {
        if (selling || max <= 0 || sel <= 0) return;
        selling = true;
        okBtn.interactable = false;

        int sell = Mathf.Min(sel, max);

        db.SetCount(index, Mathf.Max(0, max - sell));
        CurrencyManager.Instance?.AddGold(sell * unitPrice);

        var build = FindFirstObjectByType<BuildManager>(FindObjectsInactive.Exclude);
        if (build != null && build.gameObject.activeInHierarchy)
            build.UpdateCountTMP();

        confirm.gameObject.SetActive(false);
        done.gameObject.SetActive(true);
        done.anchoredPosition = Vector2.zero;
        done.SetAsLastSibling();

        selling = false;
    }

    void CloseDone()
    {
        if (done == null) return;
        done.DOKill();

        var parentRT = done.parent as RectTransform;
        float distance = ((parentRT != null) ? parentRT.rect.height : Screen.height) + done.rect.height;
        Vector2 target = done.anchoredPosition + Vector2.down * distance;

        done.DOAnchorPos(target, doneCloseDur).SetEase(Ease.InQuad)
            .OnComplete(() => { BagManager.Instance?.Rebuild(); Destroy(gameObject); });
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
        p.DOKill();
        var target = p.anchoredPosition + Vector2.down * slideOffset;
        p.DOAnchorPos(target, dur).SetEase(Ease.InQuad).OnComplete(() => onEnd?.Invoke());
    }

}
