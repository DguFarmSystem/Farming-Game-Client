// TMPro
using TMPro;

// Unity
using UnityEngine;

[DisallowMultipleComponent]
public class Friend : MonoBehaviour
{
    [Header("TMP Variables")]
    [SerializeField] private TextMeshProUGUI nameTMP;
    [SerializeField] private TextMeshProUGUI trackTMP;

    private int id;

    public void Init(int _id, string _name, string _track)
    {
        id = _id;
        nameTMP.text = _name;
        trackTMP.text = _track;
    }
}
