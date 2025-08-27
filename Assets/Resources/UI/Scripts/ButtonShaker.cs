using UnityEngine;
using System.Collections;

public class ButtonShaker : MonoBehaviour
{
    private RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        while (true)
        {
            for (int i = 0; i < 3; i++)
            {
                yield return RotateTo(5f, 0.15f);
                yield return RotateTo(-5f, 0.3f);
                yield return RotateTo(0f, 0.15f);
            }

            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerator RotateTo(float targetZ, float duration)
    {
        float elapsed = 0f;
        float startZ = rect.localEulerAngles.z;
        if (startZ > 180f) startZ -= 360f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float newZ = Mathf.Lerp(startZ, targetZ, t);
            rect.localEulerAngles = new Vector3(0, 0, newZ);
            yield return null;
        }

        rect.localEulerAngles = new Vector3(0, 0, targetZ);
    }
}
