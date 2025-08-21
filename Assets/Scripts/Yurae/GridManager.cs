// Unity
using UnityEngine;

[DisallowMultipleComponent]
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private int width = 10, height = 10;
    [SerializeField] private float cellSize = 1f;

    [Header("프리팹")]
    [SerializeField] private GameObject baseTile;
    [SerializeField] private GameObject baseGridPrefab;

    [Header("부모 오브젝트")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private Transform objectParent;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateBaseGrid();
    }

    private void GenerateBaseGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = GetWorldPosition(x, y);
                Vector2Int gridPos = GetGridPosition(pos);
                Debug.Log(gridPos);

                GameObject gridObj = Instantiate(baseGridPrefab, pos, Quaternion.identity, gridParent);

                BaseGrid grid = gridObj.GetComponent<BaseGrid>();
                if (grid != null) grid.SetGridPos(gridPos);

                GameObject tile = Instantiate(baseTile, pos, Quaternion.identity, objectParent);
                tile.transform.position = pos;

                TileData tileData = tile.GetComponent<TileData>();

                grid.PlaceTile(tileData);
            }
        }
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        float worldX = (x - y) * (cellSize / 2f);
        float worldY = (x + y) * (cellSize / 4f);
        return new Vector2(worldX, worldY);
    }

    public Vector2Int GetGridPosition(Vector2 worldPos)
    {
        float halfCellSize = cellSize / 2f;
        float quarterCellSize = cellSize / 4f;

        int x = Mathf.FloorToInt((worldPos.x / halfCellSize + worldPos.y / quarterCellSize) / 2f);
        int y = Mathf.FloorToInt((worldPos.y / quarterCellSize - worldPos.x / halfCellSize) / 2f);
        return new Vector2Int(x, y);
    }

}
