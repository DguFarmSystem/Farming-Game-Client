using UnityEngine;
using TMPro;

public class SunshineGame_Sunshine : MonoBehaviour
{
    private TMP_Text numText;
    private int num;

    void Start()
    {
        numText = transform.GetComponentInChildren<TMP_Text>();
        num = Random.Range(1, 10);
        numText.text = $"{num}";
    }

    public int GetNum()
    {
        return num;
    }

    public void Pop()
    {
        gameObject.SetActive(false);
    }
}
