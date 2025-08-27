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
        // Start 메서드에서 data를 초기화하고 x, y를 할당
        if (data == null)
        {
            data = new FarmPlotData();
        }
        data.x = this.x; // 인스펙터에 설정된 x 값을 data에 할당
        data.y = this.y; // 인스펙터에 설정된 y 값을 data에 할당
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
                // 서버에서 받은 ISO-8601 문자열을 DateTime으로 파싱
                if (DateTime.TryParse(data.planted_at, out DateTime plantedTime))
                {
                    // 서버와 동일한 성장 시간 로직 사용
                    //double growTimeSeconds = 24 * 3600 - (2 * 3600 * data.useSunCount);
                    double growTimeSeconds = 10;
                    growTimeSeconds = Mathf.Max(1, (float)growTimeSeconds);

                    double elapsedSeconds = (DateTime.UtcNow - plantedTime.ToUniversalTime()).TotalSeconds;
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
                }
                else
                {
                    spriter.sprite = seedSprite;
                }
                break;
            case "grown":
                spriter.sprite = grownSprite;
                timer_UI.SetActive(false);
                break;
        }
    }

    public void PlantSeed()
    {
        // 로컬 데이터 업데이트
        data.planted_at = FarmGroundAPI.NowIsoUtc();
        data.status = "growing";
        data.plant_name = ""; // 심을 때 작물명 초기화 (수확 시점에 지정)

        UpdateVisual();

        // 서버에 데이터 업데이트 요청 (Patch)
        StartCoroutine(FarmGroundAPI.PatchTile(data.x, data.y, FarmGroundAPI.ToDto(data), (ok, raw) =>
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

        // UIManager 로직은 그대로 유지
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.5f);
        timer_UI.transform.position = screenPos;
    }

    public void TryHarvest()
    {
        if (data.status != "grown") return;

        // 꽃을 미리 지정하고 팝업을 띄움
        data.plant_name = FlowerDataManager.Instance.GetRandomFlowerNameByRarityWeighted();

        UIManager.Instance.OpenHarvestPopup(data.plant_name);
        FlowerDataManager.Instance.RegisterFlower(data.plant_name);

        string getFlower = "Deco_" + data.plant_name;

        database.AddData(getFlower); //꽃 추가

        // 로컬 데이터 'empty' 상태로 업데이트
        data.plant_name = "";
        data.planted_at = "";
        data.status = "empty";
        data.useSunCount = 0;

        UpdateVisual();

        // 서버에 데이터 업데이트 요청 (Patch)
        StartCoroutine(FarmGroundAPI.PatchTile(data.x, data.y, FarmGroundAPI.ToDto(data), (ok, raw) =>
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

        if (DateTime.TryParse(data.planted_at, out DateTime plantedTime))
        {
            //double growTimeSeconds = 24 * 3600 - (2 * 3600 * data.useSunCount);
            double growTimeSeconds = 10;
            growTimeSeconds = Mathf.Max(1, (float)growTimeSeconds);

            double elapsed = (DateTime.UtcNow - plantedTime.ToUniversalTime()).TotalSeconds;
            double remainingSeconds = growTimeSeconds - elapsed;

            // 남은 시간 표시
            if (remainingSeconds > 0)
            {
                TimeSpan ts = TimeSpan.FromSeconds(remainingSeconds);
                string display = string.Format("{0:D2}:{1:D2}", ts.Hours, ts.Minutes);
                timer_text.text = display;
            }
            else
            {
                // 성장 완료
                Debug.Log("다 자람!");
                data.status = "grown";
                UpdateVisual();
                
                // 서버에 상태 업데이트 요청 (Patch)
                StartCoroutine(FarmGroundAPI.PatchTile(data.x, data.y, FarmGroundAPI.ToDto(data), (ok, raw) =>
                {
                    if (!ok)
                    {
                        Debug.LogError($"[FarmGround] 성장 완료 상태 업데이트 실패: {raw}");
                    }
                }));
            }
        }
    }
    
    private void OnMouseDown()
    {
        if (UIManager.Instance.IsPopupOpen())
            return;

        UIManager.Instance.ShowPlantUI(this);
    }
}