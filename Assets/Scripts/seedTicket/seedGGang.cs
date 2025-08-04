using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class seedGGang : MonoBehaviour
{
    public GameObject popupUI; //띄울 씨앗깡 유아이
    public GameObject resultUI; //결과 유아이
    public TMP_Text ticketText; //티켓 수
    public Button drawButton; // 뽑기 버튼
    public Button yesButton; // 확인 버튼
    public Button exitButton; //나가기 버튼
    public Animator chestAnimator; //
    public TMP_Text resultText; //결과 텍스트

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
        popupUI.SetActive(false);
        resultUI.SetActive(true);
        resultText.text = "";


        //chestAnimator.SetTrigger("Open");
        yield return new WaitForSeconds(1f);
        resultText.text = "X" + amount.ToString(); // 초기화
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
}
