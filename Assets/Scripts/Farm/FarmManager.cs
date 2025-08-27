using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FarmManager : MonoBehaviour
{
    [Header("씬에 이미 배치된 타일 부모(없으면 null 가능)")]
    public Transform tileParent;

    [Header("유저/식별")]
    public string uid;
    
    [Tooltip("plot_id 자동 채번 시 접두사 (기본: uid_인덱스)")]
    public string plotIdPrefixOverride = "";
    [Tooltip("임시 액세스 토큰 (테스트용)")]
    [SerializeField] private string temporaryAccessToken;

    [Header("상태별 밭 스프라이트")]
    public Sprite emptySprite;
    public Sprite seedSprite;
    public Sprite growingSprite;
    public Sprite growingSprite_1;
    public Sprite growingSprite_2;
    public Sprite grownSprite;

    private readonly Dictionary<string, FarmGround> tilesById = new Dictionary<string, FarmGround>();

    private float checkInterval = 5f;
    private float checkTimer = 0f;

    private void Start()
    {
        if (APIManager.Instance != null)
        {
            FarmGroundAPI.AccessToken = APIManager.Instance.getAccessToken();
        }
        CollectSceneTiles();
        StartCoroutine(InitFarmFromServer());
    }

    private void Update()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            CheckAllGrowth();
            UpdateAllVisual();
            checkTimer = 0f;
        }
    }

    private void CollectSceneTiles()
    {
        tilesById.Clear();

        FarmGround[] grounds = (tileParent != null)
            ? tileParent.GetComponentsInChildren<FarmGround>(true)
            : FindObjectsOfType<FarmGround>(true);

        string prefix = string.IsNullOrEmpty(plotIdPrefixOverride) ? (uid + "_") : plotIdPrefixOverride;

        for (int i = 0; i < grounds.Length; i++)
        {
            var g = grounds[i];

            g.emptySprite = emptySprite;
            g.seedSprite = seedSprite;
            g.growingSprite = growingSprite;
            g.growingSprite_1 = growingSprite_1;
            g.growingSprite_2 = growingSprite_2;
            g.grownSprite = grownSprite;

            string plotId = $"{g.x}_{g.y}";
            
            if (g.data == null)
            {
                g.data = new FarmPlotData
                {
                    uid = uid,
                    x = g.x, 
                    y = g.y,
                    plant_name = "",
                    planted_at = "",
                    status = "empty",
                    useSunCount = 0
                };
            }
            else
            {
                g.data.uid = uid;
            }

            if (!tilesById.ContainsKey(plotId))
                tilesById.Add(plotId, g);
            else
                Debug.LogWarning($"[FarmManager] 중복 plot_id 발견: {plotId} (씬 배치를 확인하세요)");
        }
        
        Debug.Log($"[FarmManager] 씬에서 수집한 타일 수: {tilesById.Count}");
    }

    private IEnumerator InitFarmFromServer()
    {
        foreach (var kv in tilesById)
        {
            var tile = kv.Value;

            bool done = false;
            FarmGroundAPI.FarmTileDto serverData = null;
            string error = null;

            yield return FarmGroundAPI.GetTile(tile.data.x, tile.data.y, (ok, dto, raw) =>
            {
                serverData = dto;
                error = ok ? null : raw;
                done = true;
            });

            if (!done)
            {
                Debug.LogError($"[FarmManager] 서버 통신 실패: {error}");
                continue;
            }

            if (serverData != null)
            {
                FarmGroundAPI.ApplyDtoToPlot(serverData, tile.data);
                tile.data.uid = uid;
                tile.InitGround(tile.data);
            }
            else
            {
                var d = tile.data;
                var dto = FarmGroundAPI.ToDto(d);
                tile.InitGround(d);

                Debug.Log(d.x + " " + d.y);

                // 밭이 없는 경우, PATCH 요청으로 생성
                yield return FarmGroundAPI.PatchTile(d.x, d.y, dto, (ok, raw) =>
                {
                    if (!ok)
                        Debug.LogError($"[FarmManager] 신규 밭 생성 실패: {raw}");
                    else
                        Debug.Log($"[FarmManager] 신규 밭 생성 성공: {raw}");
                });
            }
        }
    }

    public void CheckAllGrowth()
    {
        foreach (var tile in tilesById.Values)
            tile.CheckGrowth();
    }

    public void UpdateAllVisual()
    {
        foreach (var tile in tilesById.Values)
            tile.UpdateVisual();
    }
}