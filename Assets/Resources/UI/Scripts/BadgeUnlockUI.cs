using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BadgeUnlockUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image darkBG;        // 검은 배경
    [SerializeField] private GameObject lightFx;  // 회전 라이트
    [SerializeField] private Image badgeIcon;     // 뱃지 아이콘
    [SerializeField] private TMP_Text titleText;  // 뱃지 제목
    [SerializeField] private TMP_Text descText;   // 뱃지 설명
    [SerializeField] private Button closeButton;  // 확인 버튼

    private Action onClose;

    public void Init(Sprite icon, string title, string desc, Action onClose)
    {
        if (badgeIcon) { badgeIcon.sprite = icon; badgeIcon.preserveAspect = true; }
        if (titleText) titleText.text = title ?? "";
        if (descText) descText.text = desc ?? "";
        this.onClose = onClose;

        if (closeButton)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                this.onClose?.Invoke();
                Destroy(gameObject);
            });
        }
    }
}
