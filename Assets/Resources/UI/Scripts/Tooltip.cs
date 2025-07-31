using UnityEngine;
using TMPro;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance;

    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    private void Update()
    {
        // 마우스를 따라다니게
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            Input.mousePosition,
            null,
            out pos
        );
        tooltipRect.anchoredPosition = pos + new Vector2(10f, -10f);
    }

    public void Show(string title, string desc)
    {
        gameObject.SetActive(true);
        titleText.text = title;
        descText.text = desc;

        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = false;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
