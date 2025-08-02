using UnityEngine;

public class FlowerDataManager : MonoBehaviour
{
    public static FlowerDataManager Instance { get; private set; }

    public FlowerDataSO flowerData;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    //도감 등록 (꽃 이름 넣어서 등록 변수를 true로 만들어줌)
    public void RegisterFlower(string flowerName)
    {
        foreach (var flower in flowerData.flowerList)
        {
            if (flower.flowerName == flowerName)
            {
                flower.isRegistered = true;
                return;
                
            }
        }

        Debug.LogWarning("등록하려는 꽃 없음: " + flowerName);
    }


    //꽃이름을 받아와서 스프라이트를 가져오기
    public Sprite GetFlowerSprite(string flowerName)
    {
        var flower = flowerData.flowerList.Find(f => f.flowerName == flowerName);
        if (flower != null)
        {
            return flower.isRegistered ? flower.originalSprite : flower.silhouetteSprite;
        }
        return null;
    }
}
