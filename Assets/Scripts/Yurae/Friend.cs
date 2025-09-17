// TMPro
using TMPro;

// Unity
using UnityEngine;
using UnityEngine.UI;

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
        trackTMP.text = LinkEnumToTrack(_track);
    }

    public string LinkEnumToTrack(string _track)
    {
        switch (_track)
        {
            case "UNION":
                return "���Ͽ�";

            case "SECURITY_WEB":
                return "��/����";

            case "AI":
                return "�ΰ�����";

            case "IOT_ROBOTICS":
                return "�繰���ͳ�";

            case "BIGDATA":
                return "������";

            case "GAMING_VIDEO":
                return "����/����";

            default:
                return "Error";
        }
    }
}
