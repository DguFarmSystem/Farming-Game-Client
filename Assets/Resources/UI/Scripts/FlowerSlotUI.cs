using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlowerSlotUI : MonoBehaviour
{
    //[SerializeField] private Image imageBackground;
    [SerializeField] private Image imageIcon;              // 아이콘
    [SerializeField] private TextMeshProUGUI textName;     // 이름

    private bool collected;
    private string flowerNameCache = "";

    public void Init()
    {
        textName.text = "???";
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

        if (collected)
        {
            string display = FlowerDataManager.Instance?.GetDisplayName(flowerNameCache) ?? flowerNameCache;
            textName.text = display; 
        }
        else
        {
            textName.text = "???";
        }
    }
}
