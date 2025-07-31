using UnityEngine;

public class Ground : MonoBehaviour
{
    [Header("밭 상태")]
    public int id; // 0번 부터 1번 밭
    public bool isLock = true;          // 잠겨있는지
    public bool isPlant = false;        // 씨앗 심어져 있는지
    public bool isBloomReady = false;   // 수확 가능한지

    [Header("성장 시간")]
    public float bloomTime = 5f;        // 테스트용 5초
    private float curTime = 0f;

    [Header("꽃 정보")]
    private string flowerName;

    [Header("땅 스프라이트")]
    SpriteRenderer spriter; // 현재 땅의 스프라이트 렌더러
    public Sprite[] empty_Ground; // 빈 땅 이미지
    public Sprite[] seed_Ground; // 씨앗 땅
    public Sprite[] grow1_Ground; // 1 번째 성장 땅
    public Sprite[] grow2_Ground; // 2 번째 성장 땅
    public Sprite[] grow3_Ground; // 3 번째 성장 땅
    public Sprite[] full_Ground; //다 자란 땅
    public Sprite[] lock_Ground; //잠긴 땅

    void Start()
    {
        spriter = GetComponent<SpriteRenderer>(); //스프라이트 렌더러 넣어주기

    }

    private void Update()
    {
        Sprite_Update();
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

    private void Sprite_Update()
    {
        if (isLock) spriter.sprite = lock_Ground[id - 1]; //1번땅은 잠긴게 없어서 id-1 해야 잠긴땅 스프라이트 적용
        else
        {
            if (isPlant)
            {
                if (isBloomReady) spriter.sprite = full_Ground[id]; //다 자란 땅
                else // 안자란 땅
                {

                    if (curTime / bloomTime < 0.3) //30% 이하 일 경우
                    {
                        spriter.sprite = grow1_Ground[id]; //1 번째 자란 땅
                    }
                    else if (curTime / bloomTime < 0.6) // 60% 이하 일 경우
                    {
                        spriter.sprite = grow2_Ground[id]; // 2번째 자란 땅
                    }
                    else //그 외 60 % 이상
                    {
                        spriter.sprite = grow3_Ground[id]; //3 번째 자란땅
                    }
                }
            }
            else
            {
                spriter.sprite = empty_Ground[id]; //씨앗이 없으면 빈땅
            }

        }
    }
}
