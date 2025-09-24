// Unity
using UnityEngine;

[DisallowMultipleComponent]
public class PlacementManager : MonoBehaviour
{
    [SerializeField] private ObjectDatabase database;
    [SerializeField] private GameObject currentPrefab;
    [SerializeField] private Transform objectParent;
    [SerializeField] private ObjectSelectButton curButton;
    [SerializeField] private ObjectSelectButton lastButton;
    [SerializeField] TileSelectionUI tileSelectionUI;
    [SerializeField] private BuildManager buildManager;

    private bool canPlace;
    private GameObject ghostObject;

    [SerializeField] private bool isCarrying = false;

    void Update()
    {
        MoveObject();
    }

    private void MoveObject()
    {
        if (currentPrefab == null) return;

        if (Input.GetMouseButtonDown(1))
        {
            CancelPlace();
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2Int gridPos = GridManager.Instance.GetGridPosition(mousePos);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        // Grid 위에서만 판별
        if (hit != null && hit.CompareTag("Base Grid"))
        {
            BaseGrid grid = hit.GetComponent<BaseGrid>();

            // Catch Tile
            TileData placedTile = grid.GetTile();
            TileType tileType = placedTile.Type();

            // Object & Plant Catch
            ObjectData placedObject = grid.GetObject();
            PlantData placedPlant = grid.GetPlant();

            // Get Current Tag
            string currentTag = currentPrefab.tag;

            switch (currentTag)
            {
                // If current tag is "Tile" then Change Tile / But if tile is equal to palced tile, player can't place 
                case "Tile":
                    TileData curTile = currentPrefab.GetComponent<TileData>();

                    if (curTile.GetName() == placedTile.GetName()) CantPlace();
                    else CanPlace();

                    break;

                case "Object":
                    if (placedObject == null && placedPlant == null)
                    {
                        if (tileType == TileType.Grass) CanPlace();
                        else CantPlace();
                    }
                    else CantPlace();

                    break;

                case "Plant":
                    PlantData curPlant = currentPrefab.GetComponent<PlantData>();

                    PlantType plantType = curPlant.Type();

                    if (placedObject == null && placedPlant == null)
                    {
                        if (plantType == PlantType.Land && tileType == TileType.Field) CanPlace();
                        else if (plantType == PlantType.Water && tileType == TileType.Water) CanPlace();
                        else CantPlace();
                    }
                    else CantPlace();

                    break;

                case "Tree":
                    PlantData curTree = currentPrefab.GetComponent<PlantData>();

                    PlantType treeType = curTree.Type();

                    if (placedObject == null && placedPlant == null)
                    {
                        if (tileType == TileType.Field || tileType == TileType.Grass) CanPlace();
                        else CantPlace();
                    }
                    else CantPlace();

                    break;
            }

            // 위치 보정
            Vector2Int corGrid = grid.GetGridPos();
            ghostObject.transform.position = GridManager.Instance.GetWorldPosition(corGrid.x, corGrid.y);

            Place(corGrid, hit);
        }
        else
        {
            SetRedVisual(ghostObject);
            ghostObject.transform.position = GridManager.Instance.GetWorldPosition(gridPos.x, gridPos.y);
        }
    }

    private void Place(Vector2Int _gridPos, Collider2D _hitObject)
    {
        if (Input.GetMouseButtonDown(0) && canPlace)
        { 
            GameObject place = Instantiate(currentPrefab);
            BaseGrid grid = _hitObject.GetComponent<BaseGrid>();

            TileData placedtileData = grid.GetTile();
            PlaceableObject placedTile = placedtileData.GetComponent<PlaceableObject>();

            switch (place.tag)
            {
                case "Tile":
                    // 제거된 타일 갯수 추가
                    //BaseGrid grid = _hitObject.GetComponent<BaseGrid>();

                    //TileData placedtileData = grid.GetTile();
                    //PlaceableObject placedTile = placedtileData.GetComponent<PlaceableObject>();

                    if (database.GetCountFromID(placedTile.GetID()) >= 0)
                    {
                        //database.AddData(lastTile.GetID());
                        database.ChangeCountByID(placedTile.GetID(), 1,
                        onSuccess: () => Debug.Log("서버 동기화 성공"),
                        onError: e => Debug.LogError(e));
                    }

                    // 제거 후 타일 배치
                    DestroyImmediate(placedtileData.gameObject);
                    grid.PlaceTile(place.GetComponent<TileData>());

                    // 새로 설치된 타일 정보 로드
                    TileType tileType = grid.GetTile().Type();
                    PlaceableObject newTile = grid.GetTile().GetComponent<PlaceableObject>();

                    // 설치된 오브젝트 체크
                    PlaceableObject placed = null;
                    if (grid.GetPlant() != null) placed = grid.GetPlant().GetComponent<PlaceableObject>();
                    else if (grid.GetObject() != null) placed = grid.GetObject().GetComponent<PlaceableObject>();

                    if (placed == null)
                    {
                        // 서버에 업데이트
                        GardenControllerAPI.UpdateGardenTile(
                            _gridPos.x, _gridPos.y,
                            tileType: newTile.GetNoID(),
                            objectType: 0,
                            rotation: Garden.RotationEnum.R0,
                            onSuccess: res => Debug.Log("PATCH Success: " + res),
                            onError: err => Debug.LogError(err)
                        );
                    }
                    else
                    {
                        // 변경 후 타일이 혹시나 같은 속성일 경우는 Return / 아닐 경우 식물 회수
                        if (grid.GetPlant() != null)
                        {
                            if ((grid.GetPlant().Type() == PlantType.Land && tileType != TileType.Field) || (grid.GetPlant().Type() == PlantType.Water && tileType != TileType.Water))
                            {
                                PlaceableObject currentPlant = grid.GetPlant().GetComponent<PlaceableObject>();
                                if (database.GetCountFromID(currentPlant.GetID()) >= 0)
                                {
                                    //database.AddData(currentPlant.GetID());
                                    database.ChangeCountByID(currentPlant.GetID(), 1,
                                    onSuccess: () => Debug.Log("서버 동기화 성공"),
                                    onError: e => Debug.LogError(e));
                                }

                                GardenControllerAPI.UpdateGardenTile(
                                    _gridPos.x, _gridPos.y,
                                    tileType: grid.GetTile().GetComponent<PlaceableObject>().GetNoID(),
                                    objectType: 0,
                                    rotation: Garden.RotationEnum.R0,
                                    onSuccess: res => Debug.Log("PATCH Success: " + res),
                                    onError: err => Debug.LogError(err)
                                );

                                grid.InitPlant();
                                DestroyImmediate(currentPlant.gameObject);
                            }
                        }

                        if (grid.GetObject() != null)
                        {
                            if (tileType != TileType.Grass)
                            {
                                PlaceableObject currentObject = grid.GetObject().GetComponent<PlaceableObject>();
                                if (database.GetCountFromID(currentObject.GetID()) >= 0)
                                {
                                    //database.AddData(currentObject.GetID());
                                    database.ChangeCountByID(currentObject.GetID(), 1,
                                    onSuccess: () => Debug.Log("서버 동기화 성공"),
                                    onError: e => Debug.LogError(e));
                                }

                                GardenControllerAPI.UpdateGardenTile(
                                    _gridPos.x, _gridPos.y,
                                    tileType: grid.GetTile().GetComponent<PlaceableObject>().GetNoID(),
                                    objectType: 0,
                                    rotation: Garden.RotationEnum.R0,
                                    onSuccess: res => Debug.Log("PATCH Success: " + res),
                                    onError: err => Debug.LogError(err)
                                );

                                grid.InitObject();
                                DestroyImmediate(currentObject.gameObject);
                            }
                        }
                    }    
                    break;

                case "Object":
                    //_hitObject.GetComponent<BaseGrid>().PlaceObject(place.GetComponent<ObjectData>());
                    grid.PlaceObject(place.GetComponent<ObjectData>());

                    ObjectData objectData = grid.GetObject().GetComponent<ObjectData>();
                    PlaceableObject placedObject = objectData.GetComponent<PlaceableObject>();

                    GardenControllerAPI.UpdateGardenTile(
                        _gridPos.x, _gridPos.y,
                        tileType: placedTile.GetNoID(),
                        objectType: placedObject.GetNoID(),
                        rotation: Garden.RotationEnum.R0,
                        onSuccess: res => Debug.Log("PATCH Success: " + res),
                        onError: err => Debug.LogError(err)
                    );
                    break;

                case "Plant":   
                    //_hitObject.GetComponent<BaseGrid>().PlacePlant(place.GetComponent<PlantData>());
                    grid.PlacePlant(place.GetComponent<PlantData>());

                    PlantData plantData = grid.GetPlant().GetComponent<PlantData>();
                    PlaceableObject placedPlant = plantData.GetComponent<PlaceableObject>();

                    GardenControllerAPI.UpdateGardenTile(
                        _gridPos.x, _gridPos.y,
                        tileType: placedTile.GetNoID(),
                        objectType: placedPlant.GetNoID(),
                        rotation: Garden.RotationEnum.R0,
                        onSuccess: res => Debug.Log("PATCH Success: " + res),
                        onError: err => Debug.LogError(err)
                    );
                    break;

                case "Tree":
                    //_hitObject.GetComponent<BaseGrid>().PlacePlant(place.GetComponent<PlantData>());
                    grid.PlacePlant(place.GetComponent<PlantData>());

                    PlantData treeData = grid.GetPlant().GetComponent<PlantData>();
                    PlaceableObject placedTree = treeData.GetComponent<PlaceableObject>();

                    GardenControllerAPI.UpdateGardenTile(
                        _gridPos.x, _gridPos.y,
                        tileType: placedTile.GetNoID(),
                        objectType: placedTree.GetNoID(),
                        rotation: Garden.RotationEnum.R0,
                        onSuccess: res => Debug.Log("PATCH Success: " + res),
                        onError: err => Debug.LogError(err)
                    );
                    break;
            }

            //database.PlaceData(place.GetComponent<PlaceableObject>().GetID());

            database.ChangeCountByID(place.GetComponent<PlaceableObject>().GetID(), -1,
            onSuccess: () =>
            {
                buildManager.Init();

                // Judge Cancel
                if (database.GetCountFromID(place.GetComponent<PlaceableObject>().GetID()) == 0) CancelPlace();

                place.GetComponent<PlaceableObject>().SetPosition(_gridPos);
                place.transform.SetParent(objectParent);

                SpriteRenderer spriteRenderer = place.GetComponent<SpriteRenderer>();

                spriteRenderer.sortingOrder -= _hitObject.GetComponent<BaseGrid>().GetGridPos().x + _hitObject.GetComponent<BaseGrid>().GetGridPos().y;
            },
            onError: e => Debug.LogError(e));


        }
    }

    public void CancelPlace()
    {
        isCarrying = false;
        currentPrefab = null;
        if(ghostObject != null) DestroyImmediate(ghostObject.gameObject);
    }

    private void CantPlace()
    {
        canPlace = false;
        SetRedVisual(ghostObject);
    }

    private void CanPlace()
    {
        canPlace = true;
        SetGreenVisual(ghostObject);
    }

    public void SetPrefabToPlace(GameObject prefab)
    {
        // Count Check
        if (database.GetCountFromID(prefab.GetComponent<PlaceableObject>().GetID()) == 0)
        {
            CancelPlace();
            return;
        }

        isCarrying = true;
        currentPrefab = prefab;

        tileSelectionUI.DeslectObject();

        if (ghostObject != null) Destroy(ghostObject);
        ghostObject = Instantiate(prefab);
        DestroyImmediate(ghostObject.GetComponent<PlaceableObject>());
        SetGhostVisual(ghostObject);
    }

    void SetGhostVisual(GameObject obj)
    {
        foreach (var r in obj.GetComponentsInChildren<SpriteRenderer>())
        {
            Color c = r.color;
            c.a = 0.5f;
            r.color = c;
        }

        ghostObject.GetComponent<SpriteRenderer>().sortingOrder++;
    }

    void SetRedVisual(GameObject obj)
    {
        foreach (var r in obj.GetComponentsInChildren<SpriteRenderer>())
        {
            r.color = new Color(1f, 0f, 0f, 0.5f);
        }
    }

    void SetGreenVisual(GameObject obj)
    {
        foreach (var r in obj.GetComponentsInChildren<SpriteRenderer>())
        {
            r.color = new Color(0f, 1f, 0f, 0.5f);
        }
    }

    public void SetCurrentButton(ObjectSelectButton _button)
    {
        lastButton = curButton;
        curButton = _button;
    }

    public bool IsCarrying()
    {
        return isCarrying;
    }
}
