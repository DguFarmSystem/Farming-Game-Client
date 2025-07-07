using UnityEngine;

public class HarvestUI : MonoBehaviour
{
    private Ground targetGround;

    public void Init(Ground ground)
    {
        targetGround = ground;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(ground.transform.position + Vector3.up * 1f);
        transform.position = screenPos;
        gameObject.SetActive(true);
    }

    public void OnClickHarvest()
    {
        Debug.Log("뽑기 버튼 눌림");
        if (targetGround != null)
        {
            targetGround.Harvest();
            Hide();
        }
        else
        {
            Debug.LogWarning("뽑기 실패...");
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
