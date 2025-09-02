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

        // Grid �������� �Ǻ�
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

                    PlantType curType = curPlant.Type();

                    if (placedObject == null && placedPlant == null)
                    {
                        if (curType == PlantType.Land && tileType == TileType.Field) CanPlace();
                        else if (curType == PlantType.Water && tileType == TileType.Water) CanPlace();
                        else CantPlace();
                    }
                    else CantPlace();

                    break;
            }

            // ��ġ ����
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
                    // ���ŵ� Ÿ�� ���� �߰�
                    //BaseGrid grid = _hitObject.GetComponent<BaseGrid>();

                    //TileData placedtileData = grid.GetTile();
                    //PlaceableObject placedTile = placedtileData.GetComponent<PlaceableObject>();

                    if (database.GetCountFromID(placedTile.GetID()) >= 0)
                    {
                        //database.AddData(lastTile.GetID());
                        database.ChangeCountByID(placedTile.GetID(), 1,
                        onSuccess: () => Debug.Log("���� ����ȭ ����"),
                        onError: e => Debug.LogError(e));
                    }

                    // ���� �� Ÿ�� ��ġ
                    DestroyImmediate(placedtileData.gameObject);
                    grid.PlaceTile(place.GetComponent<TileData>());

                    TileType tileType = grid.GetTile().Type();

                    // ���� �� Ÿ���� Ȥ�ó� ���� �Ӽ��� ���� Return / �ƴ� ��� �Ĺ� ȸ��

                    if (grid.GetPlant() != null)
                    {
                        if ((grid.GetPlant().Type() == PlantType.Land && tileType != TileType.Field)||(grid.GetPlant().Type() == PlantType.Water && tileType != TileType.Water))
                        {
                            PlaceableObject currentPlant = grid.GetPlant().GetComponent<PlaceableObject>();
                            if (database.GetCountFromID(currentPlant.GetID()) >= 0)
                            {
                                //database.AddData(currentPlant.GetID());
                                database.ChangeCountByID(currentPlant.GetID(), 1,
                                onSuccess: () => Debug.Log("���� ����ȭ ����"),
                                onError: e => Debug.LogError(e));
                            }

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
                                onSuccess: () => Debug.Log("���� ����ȭ ����"),
                                onError: e => Debug.LogError(e));
                            }

                            grid.InitObject();
                            DestroyImmediate(currentObject.gameObject);
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
                        objectKind: placedObject.GetNoID(),
                        rotation: Garden.RotationEnum.R0,
                        onSuccess: res => Debug.Log("PATCH Success: " + res),
                        onError: err => Debug.LogError(err)
                    );
                    break;

                case "Plant":   
                    //_hitObject.GetComponent<BaseGrid>().PlacePlant(place.GetComponent<PlantData>());
                    grid.PlacePlant(place.GetComponent<PlantData>());

                    PlantData plantData = grid.GetObject().GetComponent<PlantData>();
                    PlaceableObject placedPlant = plantData.GetComponent<PlaceableObject>();

                    GardenControllerAPI.UpdateGardenTile(
                        _gridPos.x, _gridPos.y,
                        tileType: placedTile.GetNoID(),
                        objectKind: placedPlant.GetNoID(),
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
