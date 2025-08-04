using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;
    public static SceneLoader Instance {
        get {
            if (instance == null) {
                GameObject prefab = Resources.Load<GameObject>("SceneLoader");
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

    [Header("UI")]
    public Button leftButton;
    public Button rightButton;
    public GameObject labelPanel;
    public Sprite[] labelSprites;

    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.0f;
    public Animator transitionAnimator;

    private string[] mainScenes = {"TestScene0", "TestScene1", "TestScene2", "MiniGameEntrance"};
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

        // 현재 씬 이름 기준으로 idx 결정
        string currentSceneName = SceneManager.GetActiveScene().name;
        currentIdx = System.Array.IndexOf(mainScenes, currentSceneName);
        if (currentIdx == -1) currentIdx = 3;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;
    }

    void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        int labelIdx = System.Array.IndexOf(mainScenes, currentSceneName);
        if (labelIdx != -1) {
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
            labelPanel.SetActive(true);
            labelPanel.GetComponent<Image>().sprite = labelSprites[labelIdx];
            labelPanel.GetComponent<CanvasGroup>().alpha = 1f;

            fadeOutLabelCoroutine = StartCoroutine(FadeOutLabelCoroutine());
        } else {
            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
            labelPanel.SetActive(false);
        }
        if (labelIdx == 0) leftButton.gameObject.SetActive(false);
        if (labelIdx == mainScenes.Length-1) rightButton.gameObject.SetActive(false);
    }

    public void GoLeft() {
        Debug.Log($"go left from {currentIdx}");
        if (isTransitioning || currentIdx <= 0) return;
        
        currentIdx--;
        StartCoroutine(TransitionScene(mainScenes[currentIdx], "Left"));
    }

    public void GoRight() {
        Debug.Log($"go right from {currentIdx}");
        if (isTransitioning || currentIdx >= mainScenes.Length - 1) return;
        
        currentIdx++;
        StartCoroutine(TransitionScene(mainScenes[currentIdx], "Right"));
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

        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.interactable = true;

        isTransitioning = true;
        transitionAnimator.SetTrigger($"{direction}_Out"); // 1. 페이드아웃
        yield return new WaitForSeconds(fadeDuration);

        int labelIdx = System.Array.IndexOf(mainScenes, sceneName); // 2. 라벨, 화살표 띄우기
        if (labelIdx != -1) {
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
            labelPanel.SetActive(true);
            labelPanel.GetComponent<Image>().sprite = labelSprites[labelIdx];
            labelPanel.GetComponent<CanvasGroup>().alpha = 1f;
        } else {
            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
            labelPanel.SetActive(false);
        }
        if (labelIdx == 0) leftButton.gameObject.SetActive(false);
        if (labelIdx == mainScenes.Length-1) rightButton.gameObject.SetActive(false);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName); // 3. 씬 비동기 로딩
        asyncLoad.allowSceneActivation = false;
        while (asyncLoad.progress < 0.9f)
            yield return null;
        asyncLoad.allowSceneActivation = true;
        yield return null;

        transitionAnimator.SetTrigger($"{direction}_In"); // 4. 페이드인
        yield return new WaitForSeconds(fadeDuration);
        isTransitioning = false;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;

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
}
