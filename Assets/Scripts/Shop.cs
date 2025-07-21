using UnityEngine;

public class Shop : MonoBehaviour
{
    public GameObject[] Ground; //땅

    void Buy_0()
    {
        //1시간 영양제
        if (GameManager.Instance.money >= 100)
        {
            GameManager.Instance.money -= 100;
            InventoryManager.Instance.item_Count[0] += 1; //아이템 1개 증가
        }
    }

    void Buy_1()
    {
        //2시간 영양제
        if (GameManager.Instance.money >= 200)
        {
            GameManager.Instance.money -= 200;
            InventoryManager.Instance.item_Count[1] += 1; //아이템 1개 증가
        }
    }

    void Buy_2()
    {
        //4시간 영양제
        if (GameManager.Instance.money >= 300)
        {
            GameManager.Instance.money -= 300;
            InventoryManager.Instance.item_Count[2] += 1; //아이템 1개 증가
        }
    }

    void Buy_3()
    {
        //땅 구매
        if (GameManager.Instance.money >= 500)
        {
            GameManager.Instance.money -= 500;
            foreach (GameObject ground in Ground)
            {
                Ground gs = ground.GetComponent<Ground>(); //그라운드 스크립트 가져와주기
                if (gs.isLock == false)
                {
                    gs.isLock = true;
                    return; //하나만 해금해주고 종료
                }
            }
        }
    }
}
