using UnityEngine;
using System;

public class FarmGround : MonoBehaviour
{
    [Header("밭 상태")]
    public FarmPlotData data; //데이터 받아오기
    public SpriteRenderer spriter;

    [Header("성장 시간")]
    public DateTime bloomReadyAt; //수확 가능 시간 (기본 24시간 이후 아이템 사용시 줄어듬)

    [Header("상태별 밭 스프라이트")]
    public Sprite emptySprite;
    public Sprite growingSprite;
    public Sprite growingSprite_1;
    public Sprite growingSprite_2;
    public Sprite grownSprite;


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
                spriter.sprite = growingSprite;
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
        FarmGroundAPI.UpdatePlot(data);

    }

    public void TryHarvest()
    {
        if (data.status != "grown") return;

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
            double growTime = 24 - (2 * data.useSunCount);
            growTime = Mathf.Max(1, (float)growTime); // 최소 1초 보장
            Debug.Log("수확 시간 : " + growTime);
            
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
