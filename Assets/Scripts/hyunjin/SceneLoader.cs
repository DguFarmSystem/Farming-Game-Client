using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SceneData
{
    public string sceneName;
    public Sprite minimapSprite;
    public Sprite labelSprite;
}

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;
    public static SceneLoader Instance {
        get {
            if (instance == null) {
                GameObject prefab = Resources.Load<GameObject>("MiniGame/SceneLoader");
                if (prefab != null) {
                    GameObject go = Instantiate(prefab);
                    DontDestroyOnLoad(go);
                    instance = go.GetComponent<SceneLoader>();
                    Debug.Log("[SceneLoader] 자동 생성됨");
                } else
                    Debug.Log("[SceneLoader] 자동 생성 실패");
            }
            return instance;
        }
    }

    public List<SceneData> mainSceneList;

    [Header("Fade Settings")]
    public Animator transitionAnimator;

    private Button leftButton;
    private Button rightButton;
    private Image minimapImage;
    private GameObject labelPanel;
    private CanvasGroup fadePanel;

    private float fadeDuration = 0.3f; // public으로 빼기!!!!!!!!!!!!!!!!
    private bool isTransitioning = false;
    private int currentIdx = 0;
    private Coroutine fadeOutLabelCoroutine;

    void Awake()
    {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this) {
            Destroy(gameObject);
            return;
        }

        leftButton = transform.Find("LeftButton")?.GetComponent<Button>();
        rightButton = transform.Find("RightButton")?.GetComponent<Button>();
        minimapImage = transform.Find("Minimap")?.GetComponent<Image>();
        labelPanel = transform.Find("LabelPanel")?.gameObject;
        fadePanel = transform.Find("FadePanel")?.GetComponent<CanvasGroup>();
        if (!leftButton || !rightButton || !labelPanel || !fadePanel)
            Debug.LogWarning("[SceneLoader] cant find ui");

        // 현재 씬 이름 기준으로 idx 결정
        string currentSceneName = SceneManager.GetActiveScene().name;
        currentIdx = mainSceneList.FindIndex(data => data.sceneName == currentSceneName);
        if (currentIdx == -1) currentIdx = 3; // 미니게임일 경우 minigameEntrance로
        fadePanel.blocksRaycasts = false;
        fadePanel.interactable = false;
    }

    void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        int labelIdx = mainSceneList.FindIndex(data => data.sceneName == currentSceneName);
        if (labelIdx != -1) {
            ShowSceneLoaderUI(labelIdx);
            fadeOutLabelCoroutine = StartCoroutine(FadeOutLabelCoroutine());
        } else
            HideSceneLoaderUI();
    }

    public void GoLeft() {
        if (isTransitioning || currentIdx <= 0) return;
        
        currentIdx--;
        StartCoroutine(TransitionScene(mainSceneList[currentIdx].sceneName, "Left"));
        GameManager.Sound.SFXPlay("SFX_SceneChange");
    }

    public void GoRight() {
        if (isTransitioning || currentIdx >= mainSceneList.Count - 1) return;
        
        currentIdx++;
        StartCoroutine(TransitionScene(mainSceneList[currentIdx].sceneName, "Right"));
        GameManager.Sound.SFXPlay("SFX_SceneChange");
    }

    public void GoToMiniGame(string gameName) {
        if (isTransitioning) return;

        StartCoroutine(TransitionScene(gameName, "Fade"));
    }

    public void ExitMiniGame() {
        if (isTransitioning) return;

        StartCoroutine(TransitionScene("MiniGameEntrance", "Fade"));
    }

    IEnumerator TransitionScene(string sceneName, string direction) {
        if (!Application.CanStreamedLevelBeLoaded(sceneName)) {
            Debug.Log($"{sceneName} 없음");
            yield break;
        }
        if (fadeOutLabelCoroutine != null) {
            StopCoroutine(fadeOutLabelCoroutine);
            fadeOutLabelCoroutine = null;
        }

        fadePanel.blocksRaycasts = true;
        fadePanel.interactable = true;

        isTransitioning = true;
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName); // 1. 씬 비동기 로딩
        Debug.Log("load scene async");
        asyncLoad.allowSceneActivation = false;
        
        transitionAnimator.SetTrigger($"{direction}_Out"); // 2. 페이드아웃
        Debug.Log("set trigger out");
        yield return new WaitForSeconds(fadeDuration);

        int labelIdx = mainSceneList.FindIndex(data => data.sceneName == sceneName); // 3. 라벨, 화살표 띄우기
        if (labelIdx != -1)
            ShowSceneLoaderUI(labelIdx);
        else
            HideSceneLoaderUI();

        while (asyncLoad.progress < 0.9f) // 4. 씬 교체
            yield return null;
        asyncLoad.allowSceneActivation = true;
        yield return null;

        transitionAnimator.SetTrigger($"{direction}_In"); // 5. 페이드인
        Debug.Log("set trigger in");
        yield return new WaitForSeconds(fadeDuration);
        isTransitioning = false;
        fadePanel.blocksRaycasts = false;
        fadePanel.interactable = false;

        if (labelIdx != -1)
            fadeOutLabelCoroutine = StartCoroutine(FadeOutLabelCoroutine());
    }

    IEnumerator FadeOutLabelCoroutine() { // 애니메이션으로 빼기..
        yield return new WaitForSeconds(3f);

        CanvasGroup cg = labelPanel.GetComponent<CanvasGroup>();
        float fadeTime = 1.5f;
        float t = 0;
        while (t < fadeTime) {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            yield return null;
        }
        cg.alpha = 0f;
        labelPanel.SetActive(false);
    }

    void ShowSceneLoaderUI(int index)
    {
        leftButton.gameObject.SetActive(index > 0);
        rightButton.gameObject.SetActive(index < mainSceneList.Count - 1);
        minimapImage.sprite = mainSceneList[index].minimapSprite;
        minimapImage.gameObject.SetActive(true);
        labelPanel.GetComponent<Image>().sprite = mainSceneList[index].labelSprite;
        labelPanel.GetComponent<CanvasGroup>().alpha = 1f;
        labelPanel.SetActive(true);
    }

    void HideSceneLoaderUI()
    {
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);
        minimapImage.gameObject.SetActive(false);
        labelPanel.SetActive(false);
    }
}
