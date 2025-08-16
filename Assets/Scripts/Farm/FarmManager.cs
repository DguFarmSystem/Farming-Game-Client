using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmManager : MonoBehaviour
{
    public GameObject tilePrefab; //밭 프리팹
    public Transform tileParent; //밭 프리팹 넣을 곳
    public string uid; //user id

    private Dictionary<string, FarmGround> tiles = new Dictionary<string, FarmGround>();

    //주기적으로 확인
    private float checkInterval = 5f;
    private float checkTimer = 0f;

    [Header("상태별 밭 스프라이트")]
    public Sprite emptySprite;
    public Sprite seedSprite;
    public Sprite growingSprite;
    public Sprite growingSprite_1;
    public Sprite growingSprite_2;
    public Sprite grownSprite;

    private void Start()
    {
        StartCoroutine(InitFarm());
    }

    private void Update()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            Debug.Log("성장 체크");
            CheckAllGrowth(); //성장 체크후
            UpdateAllVisual(); //비주얼 업데이트
            checkTimer = 0f;
        }
    }


    IEnumerator InitFarm()
    {
        yield return FarmGroundAPI.LoadFarm(uid, OnFarmLoaded);
    }

    void OnFarmLoaded(List<FarmPlotData> farmList)
    {
        if (farmList == null || farmList.Count == 0)
        {
            StartCoroutine(CreateInitialFarm());
            return;
        }

        foreach (var data in farmList)
        {
            SpawnTile(data);
        }
    }

    //초기 설정
    IEnumerator CreateInitialFarm()
    {
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                FarmPlotData data = new FarmPlotData
                {
                    x = x,
                    y = y,
                    uid = uid,
                    plot_id = $"{uid}_{y * 3 + x}",
                    plant_name = "",
                    planted_at = "",
                    status = "empty",
                    is_shiny = false
                };

                SpawnTile(data);
                yield return FarmGroundAPI.CreatePlot(data);
            }
        }
    }

    Vector3 GridToWorldPosition(int x, int y)
    {
        float tileWidth = 5f;
        float tileHeight = 1.9f;

        float worldX = (x - y) * tileWidth / 2f;
        float worldY = -(x + y) * tileHeight / 2f;

        return new Vector3(worldX, worldY + 1.3f, -1);
    }
    void SpawnTile(FarmPlotData data)
    {
        Vector3 pos = GridToWorldPosition(data.x, data.y);
        GameObject go = Instantiate(tilePrefab, pos, Quaternion.identity, tileParent);
        FarmGround tile = go.GetComponent<FarmGround>();

        //땅 스프라이트 전달
        tile.emptySprite = emptySprite;
        tile.growingSprite = growingSprite;
        tile.seedSprite = seedSprite;
        tile.growingSprite_1 = growingSprite_1;
        tile.growingSprite_2 = growingSprite_2;
        tile.grownSprite = grownSprite;

        tile.InitGround(data);
        tiles[data.plot_id] = tile;
    }

    public void CheckAllGrowth()
    {
        foreach (var tile in tiles.Values)
        {
            tile.CheckGrowth();
        }
    }

    public void UpdateAllVisual()
    {
        foreach (var tile in tiles.Values)
        {
            tile.UpdateVisual();
        }
    }
    
}
