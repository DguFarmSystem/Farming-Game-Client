using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class seedTicketResultUI : MonoBehaviour
{
    public TMP_Text ticketCountText; // 프리팹에서 드래그 연결
    public Button closeButton;       // 프리팹에서 드래그 연결
    public Image white;
    public Image Black;

    public void Init(int total, Action onClose = null)
    {
        if (ticketCountText) ticketCountText.text = "X " + total;
        if (closeButton)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                onClose?.Invoke();
            });
        }
    }
}
