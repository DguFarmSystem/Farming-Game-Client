using UnityEngine;
using System;
using TMPro;
using System.Collections;

public class FarmGround : MonoBehaviour
{
    [Header("밭 ID")]
    public int x;
    public int y;

    [Header("밭 상태")]
    public FarmPlotData data; //데이터 받아오기
    public SpriteRenderer spriter;

    [Header("상태별 밭 스프라이트")]
    public Sprite emptySprite;
    public Sprite seedSprite;
    public Sprite growingSprite;
    public Sprite growingSprite_1;
    public Sprite growingSprite_2;
    public Sprite grownSprite;

    public GameObject timer_UI; //자라고 있는 상황 유아이
    public TMP_Text timer_text; //타이머 시간 텍스트

    public ObjectDatabase database; //데이터 베이스용

    void Awake()
    {
        if (data == null)
        {
            data = new FarmPlotData();
        }
        data.x = this.x;
        data.y = this.y;
    }
    
    public void InitGround(FarmPlotData newData)
    {
        data = newData;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        switch (data.status)
        {
            case "empty":
                spriter.sprite = emptySprite;
                timer_UI.SetActive(false);
                break;
            case "growing":
                // ❗ planted_at이 DateTime이므로 바로 사용
                double growTimeSeconds = 10;
                growTimeSeconds = Mathf.Max(1, (float)growTimeSeconds);
                double elapsedSeconds = (DateTime.UtcNow - data.planted_at.ToUniversalTime()).TotalSeconds;
                double progress = elapsedSeconds / growTimeSeconds;

                if (progress < 0.25)
                    spriter.sprite = seedSprite;
                else if (progress < 0.5)
                    spriter.sprite = growingSprite;
                else if (progress < 0.75)
                    spriter.sprite = growingSprite_1;
                else
                    spriter.sprite = growingSprite_2;

                timer_UI.SetActive(true);
                break;
            case "grown":
                spriter.sprite = grownSprite;
                timer_UI.SetActive(false);
                break;
        }
    }

    public void PlantSeed()
    {
        data.planted_at = DateTime.UtcNow;
        data.status = "growing";
        data.plant_name = "";
        data.useSunCount = 0;

        UpdateVisual();

        StartCoroutine(FarmGroundAPI.PatchTile(FarmGroundAPI.ToDto(data), (ok, raw) =>
        {
            if (!ok)
            {
                Debug.LogError($"[FarmGround] 씨앗 심기 실패: {raw}");
            }
            else
            {
                Debug.Log("[FarmGround] 씨앗 심기 성공.");
            }
        }));

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.5f);
        timer_UI.transform.position = screenPos;
    }

    public void TryHarvest()
    {
        if (data.status != "grown") return;

        data.plant_name = FlowerDataManager.Instance.GetRandomFlowerNameByRarityWeighted();
        string getFlowerId = "Deco_" + data.plant_name;
        
        if(database != null)
        {
            database.AddData(getFlowerId);
        }
        else
        {
            Debug.LogError("[FarmGround] ObjectDatabase가 할당되지 않았습니다!");
        }


        long flower_server = 0; // 값을 받을 변수 선언

        // TryGetIndexByID를 호출하면서 out 키워드를 사용합니다.
        if (database.TryGetIndexByID(getFlowerId, out int index))
        {
            // 함수가 true를 반환했으므로, index 변수에 올바른 값이 담겨있습니다.
            // 이제 이 값을 flower_server에 할당할 수 있습니다.
    
            // 할당된 값을 사용하여 GetStoreGoodsNumber 함수를 호출합니다.
            flower_server = database.GetStoreGoodsNumber(index);
        }
        else
        {
            // id를 찾지 못한 경우
            Debug.Log("ID를 찾을 수 없습니다.");
        }

        UIManager.Instance.OpenHarvestPopup(data.plant_name, database.GetNameFromID(getFlowerId));
        FlowerDataManager.Instance.RegisterFlower(data.plant_name, flower_server);

        data.plant_name = "";
        data.planted_at = default(DateTime); // ❗ DateTime 기본값으로 초기화
        data.status = "empty";
        data.useSunCount = 0;

        UpdateVisual();

        StartCoroutine(FarmGroundAPI.PatchTile(FarmGroundAPI.ToDto(data), (ok, raw) =>
        {
            if (!ok)
            {
                Debug.LogError($"[FarmGround] 수확 실패: {raw}");
            }
            else
            {
                Debug.Log("[FarmGround] 수확 성공.");
            }
        }));
    }

    public void CheckGrowth()
    {
        if (data.status != "growing")
        {
            timer_UI.SetActive(false);
            return;
        }

        // ❗ planted_at이 DateTime이므로 바로 사용
        double growTimeSeconds = 10;
        growTimeSeconds = Mathf.Max(1, (float)growTimeSeconds);
        double elapsed = (DateTime.UtcNow - data.planted_at.ToUniversalTime()).TotalSeconds;
        double remainingSeconds = growTimeSeconds - elapsed;

        if (remainingSeconds > 0)
        {
            TimeSpan ts = TimeSpan.FromSeconds(remainingSeconds);
            string display = string.Format("{0:D2}:{1:D2}", ts.Hours, ts.Minutes);
            timer_text.text = display;
        }
        else
        {
            Debug.Log("다 자람!");
            data.status = "grown";
            UpdateVisual();
            
            StartCoroutine(FarmGroundAPI.PatchTile(FarmGroundAPI.ToDto(data), (ok, raw) =>
            {
                if (!ok)
                {
                    Debug.LogError($"[FarmGround] 성장 완료 상태 업데이트 실패: {raw}");
                }
            }));
        }
    }
    
    private void OnMouseDown()
    {
        if (UIManager.Instance.IsPopupOpen())
            return;

        UIManager.Instance.ShowPlantUI(this);
    }
}