using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlowerSlotUI : MonoBehaviour
{
    [SerializeField] private Image imageBackground;
    [SerializeField] private Image imageIcon;
    [SerializeField] private TMP_Text textNo;
    [SerializeField] private TMP_Text textNum;

    public void Init(int index)
    {
        textNo.text = "No.";
        textNum.text = (index + 1).ToString("D3");

        // 초기엔 아이콘은 안 보이게 설정
        imageIcon.sprite = null;
        imageIcon.color = new Color(1, 1, 1, 0); // 투명
    }

    public void SetCollected(Sprite flowerSprite)
    {
        imageIcon.gameObject.SetActive(true);
        imageIcon.sprite = flowerSprite;
    }
}
