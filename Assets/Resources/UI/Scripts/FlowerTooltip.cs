using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(10000)]
public class FlowerTooltip : MonoBehaviour
{
    public static FlowerTooltip Instance;

    [Header("Refs")]
    [SerializeField] private Canvas tooltipCanvas;
    [SerializeField] private RectTransform panel;       // 검정 배경 패널
    [SerializeField] private TextMeshProUGUI label;     // 흰 글씨
    [SerializeField] private CanvasGroup cg;            // 페이드/표시 제어

    [Header("Options")]
    [SerializeField] private Vector2 padding = new Vector2(16, 10);
    [SerializeField] private Vector2 offset = new Vector2(18, -18);

    Camera uiCam;
    bool visible;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!tooltipCanvas) tooltipCanvas = GetComponentInParent<Canvas>();
        uiCam = tooltipCanvas && tooltipCanvas.renderMode == RenderMode.ScreenSpaceCamera
            ? tooltipCanvas.worldCamera : null;

        Hide();
    }

    void LateUpdate()
    {
        if (!visible) return;

        Vector2 pos = Input.mousePosition + (Vector3)offset;

        var size = panel.sizeDelta;
        float maxX = Screen.width - size.x - 4f;
        float maxY = Screen.height - size.y - 4f;
        pos.x = Mathf.Clamp(pos.x, 4f, maxX);
        pos.y = Mathf.Clamp(pos.y, 4f, maxY);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipCanvas.transform as RectTransform, pos, uiCam, out var lp);

        panel.anchoredPosition = lp;
    }

    public void Show(string text)
    {
        if (!label || !panel || !cg) return;

        label.text = text;
        label.ForceMeshUpdate();

        var pref = label.GetPreferredValues(text);
        panel.sizeDelta = new Vector2(pref.x + padding.x * 2f, pref.y + padding.y * 2f);

        cg.alpha = 1f;
        cg.blocksRaycasts = false;
        visible = true;
    }

    public void Hide()
    {
        if (!cg) return;
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        visible = false;
    }
}
