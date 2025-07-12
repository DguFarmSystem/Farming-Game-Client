using UnityEngine;
using UnityEngine.UI;

public class FertilizerButton : MonoBehaviour
{
    public Image iconImage;
    public Text countText;
    private Ground targetGround;

    public int item_Id;

    public void OnClickUse()
    {
        if (InventoryManager.Instance.Use(item_Id))
        {
            if(targetGround.isBloomReady) return; //이미 수확 가능하면 아이템 안쓰기
            switch (item_Id)
            {
                case 0:
                    targetGround.reduceTime(3600f);
                    break;
                case 1:
                    targetGround.reduceTime(3600f * 2);
                    break;
                case 2:
                    targetGround.reduceTime(3600f * 4);
                    break;
            }
            UpdateCount();
        }
        else
        {
            Debug.Log("영양제가 없습니다.");
        }
    }

    public void UpdateCount()
    {
        countText.text = InventoryManager.Instance.GetCount(item_Id).ToString();
    }

    public void setGround(Ground ground)
    {
        targetGround = ground;
    }
}
