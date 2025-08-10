using UnityEngine;
using UnityEngine.UI;

public class SellQuickMenu : MonoBehaviour
{
    [SerializeField] RectTransform panel;   // 작은 메뉴 패널
    [SerializeField] Button sellButton;     // "판매하기"
    [SerializeField] GameObject blocker;    // 화면 전체 투명 버튼(바깥 클릭시 닫기)

    // 내부 상태
    ObjectDatabase db;
    int index, unitPrice;
    SellPopup popupPrefab;                  // ★ 요게 없어서 에러였음

    void Awake()
    {
        if (blocker)
        {
            var btn = blocker.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(HideImmediate); // 바깥 클릭 → 닫기
        }
        if (sellButton) sellButton.onClick.AddListener(OnClickSell);

        HideImmediate(); // 시작은 숨김(루트는 켜두고)
        gameObject.SetActive(true);
    }

    // 마우스 위치에 메뉴 표시
    public void Show(ObjectDatabase d, int i, SellPopup p, int u, Vector2 screenPos)
    {
        db = d; index = i; popupPrefab = p; unitPrice = u;   // ★ 이름 통일

        var canvas = GetComponentInParent<Canvas>();
        RectTransform canvasRT = canvas.transform as RectTransform;

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out local
        );

        panel.anchoredPosition = local;

        if (blocker) blocker.SetActive(true);
        panel.gameObject.SetActive(true);
    }

    public void HideImmediate()
    {
        if (blocker) blocker.SetActive(false);
        if (panel) panel.gameObject.SetActive(false);
    }

    void OnClickSell()
    {
        HideImmediate();
        var canvas = GetComponentInParent<Canvas>()?.transform ?? transform;
        Instantiate(popupPrefab, canvas).Open(db, index, unitPrice);
    }
}
