using UnityEngine;

public class Ground : MonoBehaviour
{
    [Header("밭 상태")]
    public bool isLock = true;          // 잠겨있는지
    public bool isPlant = false;        // 씨앗 심어져 있는지
    public bool isBloomReady = false;   // 수확 가능한지

    [Header("성장 시간")]
    public float bloomTime = 5f;        // 테스트용 5초
    private float curTime = 0f;

    [Header("꽃 정보")]
    private string flowerName;

    private void Update()
    {
        if (isPlant && !isBloomReady)
        {
            curTime += Time.deltaTime;
            if (curTime >= bloomTime)
            {
                flowerName = GetRandomFlower();
                isBloomReady = true;
                Debug.Log($"꽃봉오리 등장: {flowerName}! 클릭해서 수확하세요.");
            }
        }
    }

    private void OnMouseDown()
    {
        Debug.Log($"클릭! isLock:{isLock}, isPlant:{isPlant}, isBloomReady:{isBloomReady}");

        if (isLock)
        {
            Debug.Log("잠겨있는 땅입니다.");
            return;
        }

        if (!isPlant)
        {
            Debug.Log("빈 땅: 심기 UI 호출");
            UIManager.Instance.ShowPlantUI(this);
        }
        else if (isBloomReady)
        {
            Debug.Log("꽃 다 자람: 수확 UI 호출");
            UIManager.Instance.ShowHarvestUI(this);
        }
        else
        {
            if (UIManager.Instance.IsItemUseUIActiveFor(this))
            {
                UIManager.Instance.HideAll();
            }
            else
            {
                UIManager.Instance.ShowItemUseUI(this);
            }
        }
    }

    public void Unlock()
    {
        isLock = false;
        Debug.Log("땅 해금 완료!");
    }

    public void Plant()
    {
        if (isPlant)
        {
            Debug.Log("이미 씨앗이 심어져 있습니다.");
            return;
        }

        isPlant = true;
        curTime = 0f;
        Debug.Log("씨앗을 심었습니다!");
    }

    public void Harvest()
    {
        if (!isBloomReady)
        {
            Debug.Log("아직 수확할 꽃이 없습니다.");
            return;
        }

        Debug.Log($"꽃 수확 완료! : {flowerName}");

        // 도감에 등록
        FlowerDataManager dex = FlowerDataManager.Instance;
        if (dex != null)
        {
            dex.RegisterFlower(flowerName);
        }

        /* 수확 팝업 띄우기
        FlowerPopup popup = FlowerPopup.Instance;
        if (popup != null && dex != null)
        {
            Sprite flowerSprite = dex.GetFlowerSprite(flowerName);
            popup.Show(flowerSprite);
        }*/

        // 상태 초기화
        isPlant = false;
        isBloomReady = false;
        flowerName = null;
        curTime = 0f;
    }

    private string GetRandomFlower()
    {
        string[] flowers = { "해바라기", "코스모스", "달리아", "봉선화", "달맞이꽃" };
        return flowers[Random.Range(0, flowers.Length)];
    }

    public void reduceTime(float time)
    {
        curTime += time;
    }
}
