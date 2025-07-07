using UnityEngine;

public class PlantUI : MonoBehaviour
{
    private Ground targetGround;

    public void Init(Ground ground)
    {
        targetGround = ground;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(ground.transform.position + Vector3.up * 1f);
        transform.position = screenPos;
        gameObject.SetActive(true);
    }

    public void OnClickPlant()
    {
        Debug.Log("심기 버튼 눌림");
        if (targetGround != null)
        {
            targetGround.Plant();
            Hide();
        }
        else
        {
            Debug.LogWarning("심기 실패...");
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
