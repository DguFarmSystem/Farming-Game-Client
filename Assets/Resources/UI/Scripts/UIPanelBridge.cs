using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class UIPanelBridge : MonoBehaviour
{
    [Header("Open/Close에 연결할 이벤트")]
    public UnityEvent onOpen;   // 여기다 패널 매니저의 Open()을 드래그로 연결
    public UnityEvent onClose;  // 여기다 패널 매니저의 Close()를 드래그로 연결

    [Header("열림 상태 판별(없어도 OK)")]
    [SerializeField] CanvasGroup cg; // 있으면 상태판별에 사용

    public bool IsOpenNow
    {
        get
        {
            if (!gameObject.activeInHierarchy) return false;
            if (cg) return cg.blocksRaycasts || cg.alpha > 0.9f;
            return true; // CanvasGroup 없으면 active로만 판단
        }
    }

    public void Open() => onOpen?.Invoke();
    public void Close() => onClose?.Invoke();
}
