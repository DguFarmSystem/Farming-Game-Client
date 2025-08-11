using UnityEngine;
using System;
using TMPro;

public class FarmGround : MonoBehaviour
{
    [Header("밭 상태")]
    public FarmPlotData data; //데이터 받아오기
    public SpriteRenderer spriter;

    [Header("성장 시간")]
    public DateTime bloomReadyAt; //수확 가능 시간 (기본 24시간 이후 아이템 사용시 줄어듬)

    [Header("상태별 밭 스프라이트")]
    public Sprite emptySprite;
    public Sprite seedSprite;
    public Sprite growingSprite;
    public Sprite growingSprite_1;
    public Sprite growingSprite_2;
    public Sprite grownSprite;

    public GameObject timer_UI; //자라고 있는 상황 유아이
    public TMP_Text timer_text; //타이머 시간 텍스트

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
                break;
            case "growing":
                if (DateTime.TryParse(data.planted_at, out DateTime plantedTime))
                {
                    double growTime = 24 * 3600 - (2 * 3600 *  data.useSunCount);
                    growTime = Mathf.Max(1, (float)growTime); // 최소 1초

                    double elapsedSeconds = (DateTime.Now - plantedTime).TotalSeconds;
                    double progress = elapsedSeconds / (growTime); // 진행률 0~1  시간으로 수정해야함 테스트라 초

                    if (progress < 0.25)
                        spriter.sprite = seedSprite; // 초기 씨앗
                    else if (progress < 0.5)
                        spriter.sprite = growingSprite;
                    else if (progress < 0.75)
                        spriter.sprite = growingSprite_1;
                    else
                        spriter.sprite = growingSprite_2;
                }
                else
                {
                    spriter.sprite = seedSprite; // 실패 시 기본
                }
                break;
            case "grown":
                spriter.sprite = grownSprite;
                break;
        }
    }
    public void PlantSeed(string plantName)
    {

        data.plant_name = plantName;
        data.planted_at = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        data.status = "growing";
        data.is_shiny = UnityEngine.Random.value < 0.05f;

        // DB에 업데이트 요청

        UpdateVisual();

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.5f);
        timer_UI.transform.position = screenPos;
        timer_UI.SetActive(true); //타이머 켜주기

        FarmGroundAPI.UpdatePlot(data);

    }

    public void TryHarvest()
    {
        if (data.status != "grown") return;

        UIManager.Instance.OpenHarvestPopup(data.plant_name);

        FlowerDataManager.Instance.RegisterFlower(data.plant_name);
        data.plant_name = "";
        data.planted_at = "";
        data.status = "empty";
        data.is_shiny = false;

        UpdateVisual();

        FarmGroundAPI.UpdatePlot(data);
    }

    public void CheckGrowth()
    {
        if (data.status != "growing") return;

        DateTime plantedTime;
        if (DateTime.TryParse(data.planted_at, out plantedTime))
        {
            //아이템 사용시 줄어들게 해야함
            //double growTime = 24 * 3600 - (2 * 3600 * data.useSunCount);
            double growTime = 3;
            growTime = Mathf.Max(1, (float)growTime); // 최소 1초 보장
            Debug.Log("수확 시간 : " + growTime);
            
            double elapsed = (DateTime.Now - plantedTime).TotalSeconds;
            double remainingSeconds = growTime - elapsed;

            // 남은 시간 표시 (0보다 작을 경우 0으로 고정)
            if (remainingSeconds > 0)
            {
                TimeSpan ts = TimeSpan.FromSeconds(remainingSeconds);
                string display = string.Format("{0:D2}:{1:D2}", ts.Hours, ts.Minutes);
                timer_text.text = display;
            }
            else
            {
                timer_UI.SetActive(false); //유아이 꺼주기 다자라면
            }

            if ((DateTime.Now - plantedTime).TotalSeconds >= growTime)
            {
                Debug.Log("다자람");
                data.status = "grown";
                UpdateVisual();
                FarmGroundAPI.UpdatePlot(data);
            }
        }
    }
    
    private void OnMouseDown()
    {
        //팝업 켜져있으면 다른 유아이는 안키키
        if (UIManager.Instance.IsPopupOpen())
            return;

        UIManager.Instance.ShowPlantUI(this);
    }
}
