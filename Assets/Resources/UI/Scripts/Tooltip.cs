using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance;

    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform backgroundRect; // = Image 오브젝트

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    private void Update()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            Input.mousePosition,
            null,
            out pos
        );

        tooltipRect.pivot = new Vector2(0, 1); // 좌상단 기준
        tooltipRect.anchoredPosition = pos + new Vector2(20f, -20f); // 약간 띄움
    }

    public void Show(string title, string desc)
    {
        gameObject.SetActive(true);
        titleText.text = title;
        descText.text = desc;

        StartCoroutine(ResizeToText());

        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator ResizeToText()
    {
        yield return null; // 한 프레임 뒤에 Layout 정보 갱신됨

        float paddingX = 40f;
        float paddingY = 30f;

        // 텍스트들의 최대 가로 길이 계산
        float maxWidth = Mathf.Max(titleText.preferredWidth, descText.preferredWidth);
        float totalHeight = titleText.preferredHeight + descText.preferredHeight;

        backgroundRect.sizeDelta = new Vector2(maxWidth + paddingX, totalHeight + paddingY);
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
