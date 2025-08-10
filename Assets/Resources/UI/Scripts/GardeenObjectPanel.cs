using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class GardeenObjectPanel : MonoBehaviour
{
    [SerializeField] private RectTransform topHUD; // 상단 UI들
    [SerializeField] private float hudDur = 0.3f;
    [SerializeField] private Ease hudEase = Ease.InOutCubic;

    private Vector2 hudShownPos;
    private Vector2 hudHiddenPos;


    [Header("Reference")]
    [SerializeField] private BuildManager buildManager;       // 버튼을 생성하는 빌드 매니저
    [SerializeField] private RectTransform openButton;        // UI 열기 버튼
    [SerializeField] private RectTransform closeButton;       // UI 닫기 버튼
    [SerializeField] private RectTransform[] panels;          // 0: Tile, 1: Object, 2: Plant

    [Header("Animation Settings")]
    [SerializeField] private float panelDur = 0.5f;
    [SerializeField] private float btnDur = 0.3f;
    [SerializeField] private Ease panelEaseIn = Ease.OutCubic;
    [SerializeField] private Ease panelEaseOut = Ease.InCubic;
    [SerializeField] private Ease btnEaseIn = Ease.OutBack;
    [SerializeField] private Ease btnEaseOut = Ease.InBack;

    [SerializeField] private Transform[] buttonParents; // 0: Tile Parent, 1: Object Parent, 2: Plant Parent

    [SerializeField] private bool placeMode = false;
    [SerializeField] private PlacementManager placementManager;
    [SerializeField] private TileSelectionUI tileSelectionUI;


    private Vector2[] shownPos;
    private Vector2[] hiddenPos;
    private Vector2 openShownPos, openHiddenPos, closeShownPos, closeHiddenPos;

    private int currentPanelIndex = 0; // 현재 열린 패널 인덱스

    private void Awake()
    {
        // 위치 프리셋 저장
        shownPos = new Vector2[panels.Length];
        hiddenPos = new Vector2[panels.Length];

        var parent = panels[0].parent as RectTransform;
        float offY = (parent ? parent.rect.height : Screen.height) + panels[0].rect.height;

        for (int i = 0; i < panels.Length; i++)
        {
            shownPos[i] = panels[i].anchoredPosition;
            hiddenPos[i] = shownPos[i] + new Vector2(0, -offY);
            panels[i].anchoredPosition = hiddenPos[i];
            panels[i].gameObject.SetActive(false);
        }

        openShownPos = openButton.anchoredPosition;
        closeShownPos = closeButton.anchoredPosition;
        openHiddenPos = openShownPos + new Vector2(0, -offY);
        closeHiddenPos = closeShownPos + new Vector2(0, -offY);

        // 초기 버튼 상태
        openButton.gameObject.SetActive(true);
        openButton.anchoredPosition = openShownPos;
        closeButton.gameObject.SetActive(false);
        closeButton.anchoredPosition = closeHiddenPos;

        // 버튼 이벤트
        openButton.GetComponent<Button>().onClick.AddListener(() => OpenPanel(0)); // 기본 타일
        closeButton.GetComponent<Button>().onClick.AddListener(OnClickClose);

        // TopHUD 원래 위치 저장
        if (topHUD != null)
        {
            hudShownPos = topHUD.anchoredPosition;
            float offYHud = topHUD.rect.height + 50f; // 화면 밖으로 올리는 거리
            hudHiddenPos = hudShownPos + new Vector2(0, offYHud);
        }
    }

    private void Start()
    {
        // 각 패널 내부 탭 버튼 바인딩
        BindTabButtons(panels[0]); // Tile 프레임 안의 탭 버튼들
        BindTabButtons(panels[1]); // Object 프레임 안의 탭 버튼들
        BindTabButtons(panels[2]); // Plant 프레임 안의 탭 버튼들
    }

    private void BindTabButtons(RectTransform frame)
    {
        AddSwitch(frame, "Tile Tap Button", 0);
        AddSwitch(frame, "Object Tap Button", 1);
        AddSwitch(frame, "Plant Tap Button", 2);
    }

    private void AddSwitch(Transform frame, string childName, int targetIndex)
    {
        var t = frame.Find(childName);
        if (t == null)
        {
            Debug.LogWarning($"[{frame.name}] '{childName}' 못 찾음");
            return;
        }
        var btn = t.GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogWarning($"'{childName}'에 Button 컴포넌트 없음");
            return;
        }

        btn.onClick.AddListener(() =>
        {
            // 패널이 닫혀 있으면 열고, 열려 있으면 전환
            if (!panels[targetIndex].gameObject.activeSelf) OpenPanel(targetIndex);
            else SwitchPanel(targetIndex);
        });
    }

    public void OpenPanel(int index)
    {
        placeMode = true;

        currentPanelIndex = index;

        if (topHUD != null)
            topHUD.DOAnchorPos(hudHiddenPos, hudDur).SetEase(hudEase);

        openButton.DOAnchorPos(openHiddenPos, btnDur).SetEase(btnEaseOut)
                  .OnComplete(() => openButton.gameObject.SetActive(false));

        panels[index].gameObject.SetActive(true);
        panels[index].anchoredPosition = hiddenPos[index];
        panels[index].DOAnchorPos(shownPos[index], panelDur).SetEase(panelEaseIn)
            .OnComplete(() =>
            {
                closeButton.gameObject.SetActive(true);
                closeButton.anchoredPosition = closeHiddenPos;
                closeButton.DOAnchorPos(closeShownPos, btnDur).SetEase(btnEaseIn);
            });

        for (int i = 0; i < panels.Length; i++)
        {
            if (i != index)
            {
                panels[i].anchoredPosition = hiddenPos[i];
                panels[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnClickClose()
    {
        placeMode = false;
        placementManager.CancelPlace();
        tileSelectionUI.DeslectObject();

        if (topHUD != null)
            topHUD.DOAnchorPos(hudShownPos, hudDur).SetEase(hudEase);

        var seq = DOTween.Sequence()
            .Join(closeButton.DOAnchorPos(closeHiddenPos, panelDur).SetEase(panelEaseOut));

        seq.Join(panels[currentPanelIndex].DOAnchorPos(hiddenPos[currentPanelIndex], panelDur).SetEase(panelEaseOut));

        seq.OnComplete(() =>
        {
            closeButton.gameObject.SetActive(false);
            panels[currentPanelIndex].gameObject.SetActive(false);

            openButton.gameObject.SetActive(true);
            openButton.anchoredPosition = openHiddenPos;
            openButton.DOAnchorPos(openShownPos, btnDur).SetEase(btnEaseIn);
        });
    }

    public void SwitchPanel(int index)
    {
        if (index == currentPanelIndex) return;

        panels[currentPanelIndex].DOKill();
        panels[index].DOKill();

        panels[currentPanelIndex].gameObject.SetActive(false);
        panels[currentPanelIndex].anchoredPosition = hiddenPos[currentPanelIndex];

        panels[index].gameObject.SetActive(true);
        panels[index].anchoredPosition = shownPos[index];

        currentPanelIndex = index;

        if (!closeButton.gameObject.activeSelf)
        {
            closeButton.gameObject.SetActive(true);
            closeButton.anchoredPosition = closeShownPos;
        }
    }

    public bool IsPlaceMode()
    {
        return placeMode;
    }

}
