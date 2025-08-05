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
        textName.text = "No."; 
        imageIcon.gameObject.SetActive(false); 
    }

    public void SetCollected(Sprite icon, string flowerName)
    {
        imageIcon.sprite = icon;
        imageIcon.gameObject.SetActive(true);
        textName.text = flowerName;
    }
}
