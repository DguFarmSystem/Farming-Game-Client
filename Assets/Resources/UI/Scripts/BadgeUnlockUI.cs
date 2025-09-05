using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BadgeUnlockUI : MonoBehaviour
{
    [Header("Badge")]
    [SerializeField] private Image badgeIcon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;

    [Header("Statue")]
    [SerializeField] private GameObject statueGroup; // Statue 루트(없으면 null 허용)
    [SerializeField] private Image statueIcon;
    [SerializeField] private TMP_Text statueName;

    [SerializeField] private Button closeButton;
    private Action onClose;

    public void Init(Sprite badgeSprite, string badgeTitle, string badgeDesc,
                     Sprite statueSprite, string statueTitle,
                     Action onClose)
    {
        if (badgeIcon) { badgeIcon.sprite = badgeSprite; badgeIcon.preserveAspect = true; }
        if (titleText) titleText.text = badgeTitle ?? "";
        if (descText) descText.text = badgeDesc ?? "";

        bool hasStatue = (statueSprite != null) || !string.IsNullOrEmpty(statueTitle);
        if (statueGroup) statueGroup.SetActive(hasStatue);
        if (hasStatue)
        {
            if (statueIcon) { statueIcon.sprite = statueSprite ?? badgeSprite; statueIcon.preserveAspect = true; }
            if (statueName) statueName.text = string.IsNullOrEmpty(statueTitle) ? (badgeTitle ?? "") : statueTitle;
        }

        this.onClose = onClose;
        if (closeButton)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => { this.onClose?.Invoke(); Destroy(gameObject); });
        }
    }
}
