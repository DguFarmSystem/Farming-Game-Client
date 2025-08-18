using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class seedTicket : MonoBehaviour
{
    [Header("유아이 관련")]
    public TMP_Text ticketCountText; //티켓 수 텍스트

    public Button closeButton; // 닫기 버튼

    [Header("조건")]
    public bool condition1;
    public bool condition3;
    public bool condition5;

    // PlayerPrefs 키
    private const string PREF_DATE = "SeedTicket_LastDate";
    private const string PREF_MASK = "SeedTicket_ClaimedMask"; // bit0=1개, bit1=3개, bit2=5개

    // 비트 마스크
    private const int BIT_1 = 1 << 0;   // 1
    private const int BIT_3 = 1 << 1;   // 2
    private const int BIT_5 = 1 << 2;   // 4
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnClickClaim(); //입장했을때 조건 보고 보상 수령


        EnsureDailyReset();
    }

    // 매일 0시 기준 초기화 (로컬 날짜 기준. 서버 붙이면 서버 날짜 사용 권장)
    private void EnsureDailyReset()
    {
        string today = DateTime.Now.ToString("yyyyMMdd");
        string last = PlayerPrefs.GetString(PREF_DATE, "");

        PlayerPrefs.SetInt(PREF_MASK, 0); // 하루치 수령 기록 초기화
        if (last != today)
        {
            PlayerPrefs.SetString(PREF_DATE, today);
            PlayerPrefs.SetInt(PREF_MASK, 0); // 하루치 수령 기록 초기화
            PlayerPrefs.Save();
        }
    }

    // 수령 버튼 클릭
    private void OnClickClaim()
    {
        EnsureDailyReset();

        int claimedMask = PlayerPrefs.GetInt(PREF_MASK, 0);
        int newMask = 0;
        int total = 0;

        // 조건 체크: 충족 && 아직 미수령이면 마스크 세팅 + 개수 합산
        if (condition1 && (claimedMask & BIT_1) == 0) { newMask |= BIT_1; total += 1; }
        if (condition3 && (claimedMask & BIT_3) == 0) { newMask |= BIT_3; total += 3; }
        if (condition5 && (claimedMask & BIT_5) == 0) { newMask |= BIT_5; total += 5; }

        if (total <= 0)
        {
            // 아무 것도 새로 받을 게 없으면 조용히 리턴 (아무 반응 X)
            // 필요하면 토스트 하나 띄우고 싶다면 여기서 처리
            Debug.Log("[SeedTicket] 오늘 추가로 받을 수 있는 뽑기권 없음.");
            return;
        }


        // 지급
        CurrencyManager.Instance.AddSeedTicket(total);

        GameObject ui = UIManager.Instance.OpenSeedTicketPopup();
        var view = ui.GetComponent<seedTicketResultUI>();

        StartCoroutine(ShowAll(ui, 0.5f, 1f, 0.5f)); // 전체 페이드인

        view.ticketCountText.text = "X " + total.ToString();
        view.Init(total, onClose: () =>
        {
            StartCoroutine(CloseUI(ui));
            // 닫힐 때 추가로 하고 싶은 처리
            // 예: UIManager.Instance.HideAll();
        });

        Debug.Log($"[SeedTicket] 오늘 새로 지급: {total}개 (누적 보유: {CurrencyManager.Instance.seedTicket})");


        // 수령 기록 저장
        claimedMask |= newMask;
        PlayerPrefs.SetInt(PREF_MASK, claimedMask);
        PlayerPrefs.Save();

    }

    // 외부 시스템에서 조건 갱신해줄 때 호출 (예: 출석 완료, 미션 완료 등)
    public void SetConditions(bool c1, bool c3, bool c5)
    {
        condition1 = c1;
        condition3 = c3;
        condition5 = c5;
    }

    public IEnumerator CloseUI(GameObject ui)
    {
        // 1. 검은 배경 페이드 아웃
        yield return StartCoroutine(ShowAll(ui, 1f, 0f, 0.5f)); // 전체 페이드인

        UIManager.Instance.HideAll(); //전부 꺼주기
    }


    private CanvasGroup EnsureCanvasGroup(GameObject root)
    {
        var cg = root.GetComponent<CanvasGroup>();
        if (!cg) cg = root.AddComponent<CanvasGroup>();
        return cg;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (!cg) yield break;
        cg.alpha = from;
        cg.interactable = false;
        cg.blocksRaycasts = (to > 0f);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;
            cg.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }
        cg.alpha = to;
        cg.interactable = (to >= 1f);
        cg.blocksRaycasts = (to > 0f);
    }

    // 통째로 페이드인
    private IEnumerator ShowAll(GameObject root, float from, float to, float duration = 0.3f)
    {
        if (!root) yield break;
        root.SetActive(true); // 꺼져있으면 알파 못 바꿈
        var cg = EnsureCanvasGroup(root);
        yield return StartCoroutine(FadeCanvasGroup(cg, from, to, duration));
    }
}
