using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class seedGGang : MonoBehaviour
{
    public GameObject popupUI; //띄울 씨앗깡 유아이
    public GameObject resultUI; //결과 유아이
    public GameObject chest; // 상자 오브젝트
    public TMP_Text ticketText; //티켓 수
    public Button drawButton; // 뽑기 버튼
    public Button yesButton; // 확인 버튼
    public Button exitButton; //나가기 버튼
    public TMP_Text resultText; //결과 텍스트
    public GameObject childGroup; //상자뺀 나머지 그룹


    public Image darkBackground; // 검은 배경
    public Image whiteFlash;     // 흰 화면 플래시

    private GameObject currentPopup; //현재 팝업창
    private void OnEnable()
    {
        // 처음엔 뽑기 UI 켜고 결과 UI는 꺼짐
        popupUI.SetActive(true);
        resultUI.SetActive(false);

        int tickets = CurrencyManager.Instance.seedTicket;
        ticketText.text = $"x {tickets}";
        drawButton.interactable = (tickets > 0);
        resultText.text = "";

        // 이벤트 중복 방지
        drawButton.onClick.RemoveAllListeners();
        drawButton.onClick.AddListener(OnClickDraw);

        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(CloseResultUI);

        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(closeAll);
    }

    public void OnClickDraw()
    {
        if (!CurrencyManager.Instance.SpendSeedTicket(1)) return;

        Debug.Log("씨앗 뽑기!");
        int amount = UnityEngine.Random.Range(1, 6);
        CurrencyManager.Instance.AddSeedCount(amount);

        StartCoroutine(PlayDrawAnimation(amount));


    }

    private IEnumerator PlayDrawAnimation(int amount)
    {
        // 기존 팝업 닫고 결과창 열기
        childGroup.SetActive(false);

        // 1. 검은 배경 페이드 인
        yield return StartCoroutine(FadeImage(darkBackground, 0f, 0.95f, 1f));

        // 2. 상자 흔들리기
        // 상자 흔들기 실행 (Transform 넘겨주기)
        yield return StartCoroutine(ShakeChest(chest.transform));

        // 4. 하얀 플래시 순간 효과
        yield return StartCoroutine(FlashWhiteScreen());

        // 5. 결과 출력
        resultUI.SetActive(true);
        resultText.text = "X" + amount.ToString();
    }
    public void ClosePopup()
    {
        popupUI.SetActive(false);
    }

    public void CloseResultUI()
    {
        UIManager.Instance.HideAll(); //전부 꺼주기
    }

    public void closeAll()
    {
        UIManager.Instance.HideAll(); //전부 꺼주기
    }

    //이미지 페이드 연출
    private IEnumerator FadeImage(Image img, float fromAlpha, float toAlpha, float duration)
    {
        Color c = img.color;
        float timer = 0f;

        while (timer < duration)
        {
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, timer / duration);
            img.color = new Color(c.r, c.g, c.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        img.color = new Color(c.r, c.g, c.b, toAlpha);
    }

    //화면 플래시 연출
    private IEnumerator FlashWhiteScreen()
    {
        whiteFlash.gameObject.SetActive(true);

        yield return FadeImage(whiteFlash, 0f, 1f, 1f);

        // 순간 밝게
        whiteFlash.color = new Color(1, 1, 1, 1f);
        yield return new WaitForSeconds(0.1f);

        // 점점 사라짐
        yield return FadeImage(whiteFlash, 1f, 0f, 1f);
        whiteFlash.gameObject.SetActive(false);
    }

    //상자 흔들림 연출
    private IEnumerator ShakeChest(Transform chestTransform, float duration = 3f, float angle = 15f, int vibrato = 6)
    {
        Quaternion originalRotation = chestTransform.localRotation;
        float elapsed = 0f;
        int direction = 1;

        while (elapsed < duration)
        {
            float shakeAngle = Mathf.Sin(elapsed * vibrato * Mathf.PI) * angle * direction;
            chestTransform.localRotation = Quaternion.Euler(0, 0, shakeAngle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        chestTransform.localRotation = originalRotation;
        chest.SetActive(false);
    }
}
