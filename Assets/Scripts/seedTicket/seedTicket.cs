using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class seedTicket : MonoBehaviour
{
    [Header("유아이 관련")]
    public TMP_Text ticketCountText; //티켓 수 텍스트
    public Button closeButton; // 닫기 버튼

    // PlayerPrefs 키
    private const string PREF_DATE = "SeedTicket_LastDate";
    private const string PREF_MASK = "SeedTicket_ClaimedMask";

    // 비트 마스크
    private const int BIT_ATTENDANCE = 1 << 0;  // 1
    private const int BIT_CHEER = 1 << 1;     // 2
    private const int BIT_FARMING = 1 << 2;   // 4

    [Header("테스트용 임시 토큰")]
    [SerializeField] private string temporaryAccessToken;
    // API 응답 데이터 구조체
    [System.Serializable]
    public class TodaySeedStatusDto
    {
        public bool isAttendance;
        public bool isCheer;
        public bool isFarmingLog;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        temporaryAccessToken = APIManager.Instance.getAccessToken();
        // 게임 시작 시 서버에서 조건 확인 및 보상 수령 로직 실행
        StartCoroutine(FetchAndClaimTickets());
    }

    private IEnumerator FetchAndClaimTickets()
    {
        // API 엔드포인트 설정
        string baseUrl = "https://api.dev.farmsystem.kr";
        string url = baseUrl + "/api/user/today-seed";

        // UnityWebRequest를 사용하여 GET 요청 생성
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            // 토큰이 필요하다면 여기에서 Authorization 헤더를 설정합니다.
            // 예: www.SetRequestHeader("Authorization", "Bearer " + "YOUR_ACCESS_TOKEN");

            // temporaryAccessToken 필드를 사용하여 헤더에 토큰 추가
            if (!string.IsNullOrEmpty(temporaryAccessToken))
            {
                www.SetRequestHeader("Authorization", "Bearer " + temporaryAccessToken);
            }

            yield return www.SendWebRequest();

            TodaySeedStatusDto status = null;
            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // 응답을 DTO로 파싱
                    status = JsonUtility.FromJson<TodaySeedStatusDto>(www.downloadHandler.text);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[SeedTicket] JSON 파싱 오류: " + ex.Message);
                    yield break;
                }
            }
            else
            {
                Debug.LogError($"[SeedTicket] 서버 통신 실패: {www.error}");
                yield break;
            }

            // 서버에서 받아온 데이터로 로직 처리
            if (status == null)
            {
                Debug.LogError("[SeedTicket] 서버에서 유효한 조건 정보를 받지 못했습니다.");
                yield break;
            }

            EnsureDailyReset();

            int claimedMask = PlayerPrefs.GetInt(PREF_MASK, 0);
            int newMask = 0;
            int total = 0;

            // 서버 조건 체크: 충족 && 아직 미수령이면 마스크 세팅 + 개수 합산
            if (status.isAttendance && (claimedMask & BIT_ATTENDANCE) == 0) { newMask |= BIT_ATTENDANCE; total += 1; }
            if (status.isCheer && (claimedMask & BIT_CHEER) == 0) { newMask |= BIT_CHEER; total += 3; }
            if (status.isFarmingLog && (claimedMask & BIT_FARMING) == 0) { newMask |= BIT_FARMING; total += 5; }

            total = 1;


            if (total <= 0)
            {
                Debug.Log("[SeedTicket] 오늘 추가로 받을 수 있는 뽑기권 없음.");
                yield break;
            }

            // 지급
            // CurrencyManager.Instance.AddSeedTicket(total); // 실제 게임 로직에 맞게 주석 해제

            // UI 팝업 처리
            GameObject ui = UIManager.Instance.OpenSeedTicketPopup();
            var view = ui.GetComponent<seedTicketResultUI>();
            
            StartCoroutine(ShowAll(ui, 0.5f, 1f, 0.5f));

            view.ticketCountText.text = "X " + total.ToString();
            view.Init(total, onClose: () =>
            {
                StartCoroutine(CloseUI(ui));
            });

            // 수령 기록 저장
            claimedMask |= newMask;
            PlayerPrefs.SetInt(PREF_MASK, claimedMask);
            PlayerPrefs.Save();
        }
    }

    private void EnsureDailyReset()
    {
        string today = DateTime.Now.ToString("yyyyMMdd");
        string last = PlayerPrefs.GetString(PREF_DATE, "");

        if (last != today)
        {
            PlayerPrefs.SetString(PREF_DATE, today);
            PlayerPrefs.SetInt(PREF_MASK, 0); // 하루치 수령 기록 초기화
            PlayerPrefs.Save();
        }
    }

    private IEnumerator CloseUI(GameObject ui)
    {
        yield return StartCoroutine(ShowAll(ui, 1f, 0f, 0.5f));
        UIManager.Instance.HideAll();
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

    private IEnumerator ShowAll(GameObject root, float from, float to, float duration = 0.3f)
    {
        if (!root) yield break;
        root.SetActive(true);
        var cg = EnsureCanvasGroup(root);
        yield return StartCoroutine(FadeCanvasGroup(cg, from, to, duration));
    }
}