using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BagItemSlotView : MonoBehaviour
{
    [SerializeField] Image icon;        // Image_Icon
    [SerializeField] TMP_Text nameTMP;  // Text_Name
    [SerializeField] TMP_Text countTMP; // Text_Count

    public void Bind(ObjectDatabase db, int index)
    {
        nameTMP.text = db.GetName(index);
        icon.sprite = db.GetSprite(index);
        countTMP.text = db.GetCountFromIndex(index).ToString();
    }

    public void RefreshCount(ObjectDatabase db, int index)
    {
        countTMP.text = db.GetCountFromIndex(index).ToString();
    }
}
