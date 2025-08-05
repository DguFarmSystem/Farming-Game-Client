// Unity
using UnityEngine;
using UnityEngine.UI;

// TMPro
using TMPro;

[DisallowMultipleComponent]
public class ObjectSelectButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameTMP;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI countTMP;

    public void Init(string _name, Sprite _sprite, int _count)
    {
        nameTMP.text = _name;
        image.sprite = _sprite;
        if (_count == -1) countTMP.text = "¹«ÇÑ";
        else countTMP.text = _count.ToString();
    }

    public void UpdateCountTMP(int _count)
    {
        countTMP.text = _count.ToString();
    }
}
