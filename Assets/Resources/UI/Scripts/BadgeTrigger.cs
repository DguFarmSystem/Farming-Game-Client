using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BadgeTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public BadgeEntry data;

    //private Image img;

    /*void Awake()
    {
        img = GetComponent<Image>();
    }*/

    public void SetData(BadgeEntry entry) => data = entry;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data == null || Tooltip.Instance == null) return;

        //if (img != null && img.color.a < 0.9f) return;

        Tooltip.Instance.Show(data.title, data.description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltip.Instance?.Hide();
    }
}