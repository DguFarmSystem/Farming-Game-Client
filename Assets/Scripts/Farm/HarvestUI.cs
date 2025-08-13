using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HarvestUI : MonoBehaviour
{
    public GameObject seed; //씨앗 오브젝트
    public GameObject resultUI; //결과 유아이

    public Button yesButton; // 확인 버튼

    public TMP_Text flower_Text; //꽃 이름

    public Image darkBackground; // 검은 배경
    public Image whiteFlash;     // 흰 화면 플래시
    public Image flower_image; //꽃 이미지


    //테두리 유아이
    public Image Collect_UI; //테두리 이미지
    public Sprite normal;
    public Sprite Rare;
    public Sprite Epic;
    public Sprite Legend;

    private void OnEnable()
    {
        resultUI.SetActive(false);

        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(CloseResultUI);

        StartCoroutine(PlayDrawAnimation());
    }

    private IEnumerator PlayDrawAnimation()
    {
        // 1. 검은 배경 페이드 인
        yield return StartCoroutine(FadeImage(darkBackground, 0f, 0.95f, 1f));
        // 2. 씨앗
        yield return StartCoroutine(shakeSeed(seed.transform));

        // 4. 하얀 플래시 순간 효과
        yield return StartCoroutine(FlashWhiteScreen());

        // 5. 결과 출력
        resultUI.SetActive(true);
    }

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

    //씨앗 흔들림 연출
    private IEnumerator shakeSeed(Transform chestTransform, float duration = 2f, float angle = 10f, int vibrato = 3)
    {
        Quaternion originalRotation = chestTransform.localRotation;
        float elapsed = 0f;
        int direction = 1;
        Debug.Log("씨앗 움직이기");

        while (elapsed < duration)
        {
            float shakeAngle = Mathf.Sin(elapsed * vibrato * Mathf.PI) * angle * direction;
            chestTransform.localRotation = Quaternion.Euler(0, 0, shakeAngle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        chestTransform.localRotation = originalRotation;
        Debug.Log("씨앗 움직이기 끝");
        seed.SetActive(false);
    }
    

    public void CloseResultUI()
    {
        UIManager.Instance.HideAll(); //전부 꺼주기
    }
}
