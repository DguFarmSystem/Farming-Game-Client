// Unity
using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using System.Collections;

[DisallowMultipleComponent]
public class FadeManager : MonoBehaviour
{
    [SerializeField] private GameObject dim;     // Dim 오브젝트 (캔버스 하위에 검은 Image)
    [SerializeField] private Image dimImage;     // Dim 오브젝트에 붙은 Image 컴포넌트

    [SerializeField] private float fadeDuration = 1.0f; // 페이드 시간(초)

    private Coroutine fadeRoutine;

    public void FadeIn(System.Action onComplete = null)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Fade(1f, 0f, fadeDuration, onComplete));
    }

    public void FadeOut(System.Action onComplete = null)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Fade(0f, 1f, fadeDuration, onComplete));
    }

    public void FadeInOut(System.Action onComplete = null)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeInOutRoutine(onComplete));
    }

    private IEnumerator FadeInOutRoutine(System.Action onComplete)
    {
        // 먼저 어두워짐
        yield return Fade(0f, 1f, fadeDuration, null);

        yield return new WaitForSeconds(1f);
        // 다시 밝아짐
        yield return Fade(1f, 0f, fadeDuration, null);

        onComplete?.Invoke();
    }

    private IEnumerator Fade(float from, float to, float duration, System.Action onComplete)
    {
        dim.SetActive(true);
        Color color = dimImage.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(from, to, t);
            dimImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        dimImage.color = new Color(color.r, color.g, color.b, to);

        if (to <= 0f) dim.SetActive(false);

        onComplete?.Invoke();
    }
}
