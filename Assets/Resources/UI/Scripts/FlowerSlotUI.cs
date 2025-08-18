using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FlowerSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [SerializeField] private Image imageBackground;
    [SerializeField] private Image imageIcon;          // Image_Icon
    [SerializeField] private TextMeshProUGUI textNo;   // 숫자
    [SerializeField] private TextMeshProUGUI textName; // "No." 고정 표기

    private bool collected;
    private string flowerNameCache = "";
    private int indexCache;

    public void Init(int index)
    {
        indexCache = index;
        textNo.text = (index + 1).ToString();
        textName.text = "No.";
        imageIcon.gameObject.SetActive(false);
        collected = false;
        flowerNameCache = "";
    }

    public void SetSprite(Sprite sprite, bool isCollected, string flowerName)
    {
        collected = isCollected;
        flowerNameCache = flowerName;

        imageIcon.sprite = sprite;
        imageIcon.preserveAspect = true;
        imageIcon.gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (collected && !string.IsNullOrEmpty(flowerNameCache))
            FlowerTooltip.Instance?.Show(flowerNameCache);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FlowerTooltip.Instance?.Hide();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        // 위치 업데이트는 TooltipController가 LateUpdate에서 처리
    }
}
