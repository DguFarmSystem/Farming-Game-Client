using UnityEngine;
using System;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class FarmGround : MonoBehaviour
{
    [Header("밭 ID")]
    public int x;
    public int y;

    [Header("밭 상태")]
    public FarmPlotData data; //데이터 받아오기
    public SpriteRenderer spriter;
    double growTimeSeconds;

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

    void Start()
    {
        if (data == null)
        {
            data = new FarmPlotData();
        }
        data.x = this.x;
        data.y = this.y;

        // 게임 시작 시 서버에서 밭 데이터를 로드
        StartCoroutine(LoadTileData());
    }
    
    // 서버에서 밭 데이터를 로드하는 코루틴
    private IEnumerator LoadTileData()
    {
        yield return StartCoroutine(FarmGroundAPI.GetTile(this.x, this.y, (ok, dto, error) =>
        {
            if (ok)
            {
                // API에서 받은 DTO를 FarmPlotData로 변환 및 적용
                FarmGroundAPI.ApplyDtoToPlot(dto, data);
                UpdateVisual();
                Debug.Log($"[FarmGround] Tile ({x},{y}) loaded successfully.");
            }
            else
            {
                Debug.LogError($"[FarmGround] Failed to load tile ({x},{y}): {error}");
                // 로드 실패 시 기본 상태로 초기화
                data.status = "empty";
                UpdateVisual();
            }
        }));
    }

    public void InitGround(FarmPlotData newData)
    {
        // 이 함수는 외부에서 데이터를 받아올 때 사용됩니다.
        // 현재는 LoadTileData에서 직접 API를 호출하므로 이 함수를 호출할 필요가 없습니다.
        data = newData;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.5f);
        timer_UI.transform.position = screenPos;
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
                growTimeSeconds = 3600 * 24 - (2 * 3600 * data.useSunCount);
                growTimeSeconds = Mathf.Max(1, (float)growTimeSeconds);
                
                // 데이터의 시간을 UTC로 변환하여 현재 UTC 시간과 비교
                double elapsedSeconds = (DateTime.UtcNow - data.planted_at).TotalSeconds;
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

    public void PlantSeed(int useSun)
    {
        data.planted_at = DateTime.UtcNow; // 심은 시간은 항상 UTC
        data.status = "growing";
        data.plant_name = "";
        data.useSunCount = useSun;

        growTimeSeconds = 3600 * 24 - (2 * 3600 * useSun);
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
            database.ChangeCountByID(getFlowerId, 1);
        }
        else
        {
            Debug.LogError("[FarmGround] ObjectDatabase가 할당되지 않았습니다!");
        }

        long flower_server = 0;
        if (database.TryGetIndexByID(getFlowerId, out int index))
        {
            flower_server = database.GetStoreGoodsNumber(index);
        }
        else
        {
            Debug.Log("ID를 찾을 수 없습니다.");
        }

        UIManager.Instance.OpenHarvestPopup(data.plant_name, database.GetNameFromID(getFlowerId));
        FlowerDataManager.Instance.RegisterFlower(data.plant_name, flower_server);

        data.plant_name = "";
        data.planted_at = default(DateTime); // 기본값으로 초기화
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

        growTimeSeconds = 3600 * 24 - (2 * 3600 * data.useSunCount);
        growTimeSeconds = Mathf.Max(1, (float)growTimeSeconds);
        // data.planted_at은 이미 UTC이므로 ToUniversalTime() 호출 필요 없음
        double elapsed = (DateTime.UtcNow - data.planted_at).TotalSeconds;
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
    
    private void OnMouseEnter()
    {
        if (UIManager.Instance.IsPopupOpen())
        {
            return;
        }
        UIManager.Instance.ShowPlantUI(this);
    }

    private void OnMouseExit()
    {
        UIManager.Instance.HidePlantUI();
    }
}