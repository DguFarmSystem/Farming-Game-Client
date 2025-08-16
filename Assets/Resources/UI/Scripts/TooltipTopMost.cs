using UnityEngine;
using UnityEngine.UI;

public class TooltipTopMost : MonoBehaviour
{
    [SerializeField] private Canvas tooltipCanvas; // 툴팁 프리팹의 Canvas 드래그

    void OnEnable()
    {
        if (!tooltipCanvas) tooltipCanvas = GetComponentInChildren<Canvas>(true);
        if (!tooltipCanvas) return;

        var canvases = FindObjectsOfType<Canvas>(true);
        Canvas top = null; int best = int.MinValue;
        foreach (var c in canvases)
        {
            int score = (c.renderMode == RenderMode.ScreenSpaceOverlay ? 100000 : 0) + c.sortingOrder;
            if (score > best) { best = score; top = c; }
        }
        if (!top) return;

        transform.SetParent(top.transform, false);

        tooltipCanvas.renderMode = top.renderMode;
        tooltipCanvas.worldCamera = top.worldCamera;
        tooltipCanvas.sortingLayerID = top.sortingLayerID;
        tooltipCanvas.targetDisplay = top.targetDisplay;

        tooltipCanvas.overrideSorting = true;
        tooltipCanvas.sortingOrder = top.sortingOrder + 5000;
    }
}
