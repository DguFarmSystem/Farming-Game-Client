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
                return "유니온";

            case "SECURITY_WEB":
                return "웹/보안";

            case "AI":
                return "인공지능";

            case "IOT_ROBOTICS":
                return "사물인터넷";

            case "BIGDATA":
                return "빅데이터";

            case "GAMING_VIDEO":
                return "게임/영상";

            default:
                return "Error";
        }
    }
}
