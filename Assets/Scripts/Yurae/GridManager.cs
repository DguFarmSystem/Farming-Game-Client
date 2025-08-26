// Unity
using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private int width = 10, height = 10;
    [SerializeField] private float cellSize = 1f;

    [Header("������")]
    [SerializeField] private GameObject baseTile;
    [SerializeField] private GameObject baseGridPrefab;

    [Header("�θ� ������Ʈ")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private Transform objectParent;

    private List<GameObject> gridObjects;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gridObjects = new List<GameObject>();
        GenerateBaseGrid(GameManager.Instance.playerLV);
    }

    private void GenerateBaseGrid(int _playerLevel = 1)
    {
        foreach (GameObject gridObject in gridObjects)
        {
            DestroyImmediate(gridObject.gameObject);
        }

        gridObjects.Clear();

        width = GetSize(_playerLevel);
        height = GetSize(_playerLevel);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = GetWorldPosition(x, y);
                Vector2Int gridPos = GetGridPosition(pos);

                GameObject gridObj = Instantiate(baseGridPrefab, pos, Quaternion.identity, gridParent);
                gridObjects.Add(gridObj);

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

    private int GetSize(int _playerLevel)
    {
        switch (_playerLevel)
        {
            case 1:
                return 10;
            case 2:
                return 20;
            case 3:
                return 30;
            default:
                Debug.LogError("Player Level Error!");
                return 0;
        }
    }

    public void LevelUp()
    {
        GameManager.Instance.playerLV++;
        GameManager.Scene.ReLoad();
    }

    public bool HasFenceAt(Vector2Int gridPos)
    {
        BaseGrid baseGrid = null;
        foreach(GameObject gridObject in gridObjects)
        {
            BaseGrid tempBG = gridObject.GetComponent<BaseGrid>();
            if (tempBG.GetGridPos() == gridPos) baseGrid = tempBG;
        }

        if (baseGrid == null) return false;

        if (baseGrid.GetObject() == null) return false;
        else if (baseGrid.GetObject().gameObject.name.Contains("Fence")) return true;

        return false;

    }
}
