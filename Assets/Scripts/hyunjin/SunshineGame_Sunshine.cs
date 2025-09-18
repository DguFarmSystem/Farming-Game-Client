using UnityEngine;
using TMPro;
using System.Collections;

public class SunshineGame_Sunshine : MonoBehaviour
{
    private int num;
    private TMP_Text numText;
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;

    private float duration = 0.5f;
    private float duration_fade = 0.3f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        numText = transform.GetComponentInChildren<TMP_Text>();
        num = Random.Range(1, 10);
        numText.text = $"{num}";
    }

    public int GetNum()
    {
        return num;
    }

    public void Pop()
    {
        col.isTrigger = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;
        Vector2 randomDir = (Vector2.up + Random.insideUnitCircle * 0.4f).normalized;
        rb.AddForce(randomDir * 8, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-5, 5), ForceMode2D.Impulse); // 회전
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(duration);
        float t = 0f;
        Color startColor = sr.color;
        Color goalColor = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);

        while(t < duration_fade) {
            t += Time.deltaTime;
            sr.color = Color.Lerp(startColor, goalColor, t/duration_fade);
            yield return null;
        }
        Destroy(gameObject);
    }
}
