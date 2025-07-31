using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BadgeTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public BadgeData data;
    private Image image;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Mouse Enter on: {gameObject.name}");

        if (Tooltip.Instance == null) return;

        var image = GetComponent<Image>();
        if (image != null && image.color.a > 0.9f)
        {
            Tooltip.Instance.Show(data.title, data.description);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltip.Instance?.Hide();
    }
}
