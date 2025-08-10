// Unity
using UnityEngine;
using UnityEngine.UI;

// TMPro
using TMPro;

[DisallowMultipleComponent]
public class ObjectSelectButton : MonoBehaviour
{
    [SerializeField] private string id;
    [SerializeField] private TextMeshProUGUI nameTMP;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI countTMP;
    [SerializeField] private ObjectDatabase database;

    public void Init(string _id, string _name, Sprite _sprite, int _count)
    {
        id = _id; 
        nameTMP.text = _name;
        image.sprite = _sprite;
        if (_count == -1) countTMP.text = "무한";
        else countTMP.text = _count.ToString();
    }

    public void UpdateCountTMP(int _count)
    {
        countTMP.text = _count.ToString();
    }

    public void UpdateCountTMPFromDatabse()
    {
        Debug.Log(database.GetCountFromID(id));

        if (database.GetCountFromID(id) == -1) countTMP.text = "무한";
        else countTMP.text = database.GetCountFromID(id).ToString();
    }
}
