using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[DefaultExecutionOrder(10000)]
public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance;

    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private Canvas tooltipCanvas;

    Coroutine resizeCo;
    Canvas rootCanvas;
    Camera uiCam;

    void Awake()
    {
        if (!tooltipRect) tooltipRect = GetComponent<RectTransform>();
        if (!canvasGroup) canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        if (!tooltipCanvas) tooltipCanvas = GetComponentInChildren<Canvas>(true);

        rootCanvas = GetComponentInParent<Canvas>();
        uiCam = rootCanvas ? rootCanvas.worldCamera : null;

        Instance = this;
        HideImmediate();
    }

    void OnEnable()
    {
        StartCoroutine(DeferredTopmost());
        Instance = this;
    }

    IEnumerator DeferredTopmost()
    {
        yield return null;
        EnsureTopmost();
    }

    private void EnsureTopmost()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        if (!rootCanvas) return;

        if (!tooltipCanvas) tooltipCanvas = GetComponentInChildren<Canvas>(true);

        int top = 0;
        foreach (var c in FindObjectsOfType<Canvas>(true))
            top = Mathf.Max(top, c.sortingOrder + (c.renderMode == RenderMode.ScreenSpaceOverlay ? 10000 : 0));

        tooltipCanvas.renderMode = rootCanvas.renderMode;
        tooltipCanvas.worldCamera = rootCanvas.worldCamera;
        tooltipCanvas.overrideSorting = true;
        tooltipCanvas.sortingOrder = top + 10;

        transform.SetAsLastSibling();

        uiCam = rootCanvas.worldCamera;
    }

    void OnDisable()
    {
        if (resizeCo != null) { StopCoroutine(resizeCo); resizeCo = null; }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        resizeCo = null; canvasGroup = null;
    }

    void Update()
    {
        if (!isActiveAndEnabled || !tooltipRect || !transform.parent) return;

        var parentRect = transform.parent as RectTransform;
        if (!parentRect) return;

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            Input.mousePosition,
            uiCam,
            out pos
        );

        tooltipRect.pivot = new Vector2(0, 1);
        tooltipRect.anchoredPosition = pos + new Vector2(20f, -20f);
    }

    public void Show(string title, string desc)
    {
        EnsureTopmost();

        gameObject.SetActive(true);
        if (titleText) titleText.text = title ?? "";
        if (descText) descText.text = desc ?? "";

        if (resizeCo != null) StopCoroutine(resizeCo);
        resizeCo = StartCoroutine(ResizeToText());

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
    }

    IEnumerator ResizeToText()
    {
        yield return null;
        if (!this || !backgroundRect || !titleText || !descText) yield break;

        const float padX = 40f, padY = 30f;
        float maxW = Mathf.Max(titleText.preferredWidth, descText.preferredWidth);
        float h = titleText.preferredHeight + descText.preferredHeight;

        backgroundRect.sizeDelta = new Vector2(maxW + padX, h + padY);
        resizeCo = null;
    }

    public void Hide()
    {
        if (resizeCo != null) { StopCoroutine(resizeCo); resizeCo = null; }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public void HideImmediate()
    {
        if (resizeCo != null) { StopCoroutine(resizeCo); resizeCo = null; }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
