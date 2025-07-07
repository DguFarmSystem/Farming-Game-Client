using UnityEngine;

public class ItemUseUI : MonoBehaviour
{
    private Ground targetGround;

    public void Init(Ground ground)
    {
        Debug.Log("ItemUseUI Init 호출됨");
        targetGround = ground;

        FertilizerButton[] list = GetComponentsInChildren<FertilizerButton>();
        foreach (var childScript in list)
        {
            childScript.setGround(ground);
        }
        
        gameObject.SetActive(true);
    }

    public bool IsVisibleFor(Ground ground)
    {
        return gameObject.activeSelf && targetGround == ground;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
