// Unity
using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private int width = 0, height = 0;
    [SerializeField] private float cellSize = 1f;

    [Header("프리팹")]
    [SerializeField] private GameObject baseTile;
    [SerializeField] private GameObject baseGridPrefab;

    [Header("부모 오브젝트")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private Transform objectParent;

    [Header("설치 오브젝트 프리팹")]
    [SerializeField] private PlaceableObject[] tilePrefabs;
    [SerializeField] private PlaceableObject[] objectPrefabs;
    [SerializeField] private PlaceableObject[] plantPrefabs;

    private List<GameObject> gridObjects;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gridObjects = new List<GameObject>();
        Build();
    }

    public void Build()
    {
        // Get Data
        PlayerControllerAPI.GetPlayerDataFromServer(
        data =>
        {
            width = GetSize(data.level);
            height = GetSize(data.level);

            BuildBaseMap();
            LoadDataFromServer();
        },
        error => Debug.LogError(error)
        );
    }

    private void BuildBaseMap()
    {
        // Initialize Grid Tile
        foreach (GameObject gridObject in gridObjects)
        {
            DestroyImmediate(gridObject.gameObject);
        }

        gridObjects.Clear();

        // Initialize Objects
        Transform[] objects = objectParent.GetComponentsInChildren<Transform>();
        foreach (Transform obj in objects)
        {
            if (obj == objectParent) continue;

            DestroyImmediate(obj.gameObject);
        }

        // Set Base Grid And Base(Grass) Tile
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

    private BaseGrid GetBaseGrid(Vector2Int pos)
    {
        foreach (GameObject gridObject in gridObjects)
        {
            BaseGrid grid = gridObject.GetComponent<BaseGrid>();

            if (grid.GetGridPos() == pos) return grid;
        }

        return null;
    }

    public void LoadDataFromServer(bool isFriendData = false, long userId = 0)
    {
        if (isFriendData)
        {
            BuildBaseMap();

            // Set Player Tile From Server
            GardenControllerAPI.GetFriendGardenDataFromServer(
                userId,
            (gardens, objects) =>
            {
                //Debug.Log($"타일 개수: {gardens.Count}");
                for (int i = 0; i < gardens.Count; i++)
                {
                    int x = gardens[i].x;
                    int y = gardens[i].y;

                    // Get Tile
                    PlaceableObject tileObj = FindTile(gardens[i].tileType);
                    TileData tile = tileObj.GetComponent<TileData>();


                    PlaceableObject obj = null;
                    PlaceableObject plant = null;

                    if (objects[i] != null)
                    {
                        obj = FindObject(objects[i].objectKind);
                        plant = FindPlant(objects[i].objectKind);
                    }

                    // Get Base Grid
                    BaseGrid grid = GetBaseGrid(new Vector2Int(x, y));

                    // Place Tile
                    TileData placedTile = grid.GetTile();
                    DestroyImmediate(placedTile.gameObject); // Remove

                    GameObject tilePrefab = Instantiate(tileObj.gameObject);
                    tilePrefab.GetComponent<PlaceableObject>().SetPosition(new Vector2Int(x, y));
                    tilePrefab.transform.SetParent(objectParent);

                    grid.PlaceTile(tilePrefab.GetComponent<TileData>());


                    // Place Object or Plant
                    if (obj != null)
                    {
                        GameObject prefab = Instantiate(obj.gameObject);
                        prefab.GetComponent<PlaceableObject>().SetPosition(new Vector2Int(x, y));
                        prefab.GetComponent<PlaceableObject>().SetRotation(objects[i].rotation);
                        prefab.transform.SetParent(objectParent);

                        prefab.GetComponent<SpriteRenderer>().sortingOrder -= x + y;

                        grid.PlaceObject(prefab.GetComponent<ObjectData>());
                    }
                    else if (plant != null)
                    {
                        GameObject prefab = Instantiate(plant.gameObject);
                        prefab.GetComponent<PlaceableObject>().SetPosition(new Vector2Int(x, y));
                        prefab.GetComponent<PlaceableObject>().SetRotation(objects[i].rotation);
                        prefab.transform.SetParent(objectParent);

                        prefab.GetComponent<SpriteRenderer>().sortingOrder -= x + y;

                        grid.PlacePlant(prefab.GetComponent<PlantData>());
                    }
                }
            },
            error => Debug.LogError(error)
            );
        }
        else
        {
            // Set Player Tile From Server
            GardenControllerAPI.GetGardenDataFromServer(
            (gardens, objects) =>
            {
                //Debug.Log($"타일 개수: {gardens.Count}");
                for (int i = 0; i < gardens.Count; i++)
                {
                    int x = gardens[i].x;
                    int y = gardens[i].y;

                    // Get Tile
                    PlaceableObject tileObj = FindTile(gardens[i].tileType);
                    TileData tile = tileObj.GetComponent<TileData>();


                    PlaceableObject obj = null;
                    PlaceableObject plant = null;

                    if (objects[i] != null)
                    {
                        obj = FindObject(objects[i].objectKind);
                        plant = FindPlant(objects[i].objectKind);
                    }

                    // Get Base Grid
                    BaseGrid grid = GetBaseGrid(new Vector2Int(x, y));

                    // Place Tile
                    TileData placedTile = grid.GetTile();
                    DestroyImmediate(placedTile.gameObject); // Remove

                    GameObject tilePrefab = Instantiate(tileObj.gameObject);
                    tilePrefab.GetComponent<PlaceableObject>().SetPosition(new Vector2Int(x, y));
                    tilePrefab.transform.SetParent(objectParent);

                    grid.PlaceTile(tilePrefab.GetComponent<TileData>());

                    // Place Object or Plant
                    if (obj != null)
                    {
                        GameObject prefab = Instantiate(obj.gameObject);
                        prefab.GetComponent<PlaceableObject>().SetPosition(new Vector2Int(x, y));
                        prefab.GetComponent<PlaceableObject>().SetRotation(objects[i].rotation);
                        prefab.transform.SetParent(objectParent);

                        prefab.GetComponent<SpriteRenderer>().sortingOrder -= x + y;

                        grid.PlaceObject(prefab.GetComponent<ObjectData>());
                    }
                    else if (plant != null)
                    {
                        GameObject prefab = Instantiate(plant.gameObject);
                        prefab.GetComponent<PlaceableObject>().SetPosition(new Vector2Int(x, y));
                        prefab.GetComponent<PlaceableObject>().SetRotation(objects[i].rotation);
                        prefab.transform.SetParent(objectParent);

                        prefab.GetComponent<SpriteRenderer>().sortingOrder -= x + y;

                        grid.PlacePlant(prefab.GetComponent<PlantData>());
                    }
                }
            },
            error => Debug.LogError(error)
            );
        }

    }

    private PlaceableObject FindObject(long id)
    {
        foreach (PlaceableObject obj in objectPrefabs)
        {
            if (obj.GetNoID() == id) return obj;
        }

        return null;
    }

    private PlaceableObject FindTile(long id)
    {
        foreach (PlaceableObject tile in tilePrefabs)
        {
            if (tile.GetNoID() == id) return tile;
        }

        Debug.LogError("알 수 없는 타일");
        return null;
    }

    private PlaceableObject FindPlant(long id)
    {
        foreach (PlaceableObject plant in plantPrefabs)
        {
            if (plant.GetNoID() == id) return plant;
        }
        return null;
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

    public void LevelUpTo(int serverLevel)
    {
        GameManager.Instance.playerLV = serverLevel;
        int size = GetSize(serverLevel);
        width = size;
        height = size;

        BuildBaseMap();       // ??/??? ???
        LoadDataFromServer(); // ?? ?? ?? ??
    }

    public bool HasFenceAt(Vector2Int gridPos)
    {
        BaseGrid baseGrid = null;
        foreach (GameObject gridObject in gridObjects)
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
