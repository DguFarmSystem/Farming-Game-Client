using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlowerSlotUI : MonoBehaviour
{
    [SerializeField] private Image imageBackground;
    [SerializeField] private Image imageIcon;
    [SerializeField] private TextMeshProUGUI textNo;
    [SerializeField] private TextMeshProUGUI textName;

    public void Init(int index)
    {
        textNo.text = (index + 1).ToString();
        textName.text = "???"; // 초기에는 미수집 상태
        imageIcon.gameObject.SetActive(false); // 아이콘 비활성화
    }

    public void SetCollected(Sprite icon, string flowerName)
    {
        imageIcon.sprite = icon;
        imageIcon.gameObject.SetActive(true);
        textName.text = flowerName;
    }
}
