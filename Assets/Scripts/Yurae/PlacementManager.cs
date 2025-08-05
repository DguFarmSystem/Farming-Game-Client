// Unity
using UnityEngine;

[DisallowMultipleComponent]
public class PlacementManager : MonoBehaviour
{
    [SerializeField] private ObjectDatabase database;
    [SerializeField] private GameObject currentPrefab;
    [SerializeField] private Transform objectParent;
    [SerializeField] private ObjectSelectButton curButton;

    private bool canPlace;
    private GameObject ghostObject;

    void Update()
    {
        MoveObject();
    }

    private void MoveObject()
    {
        if (currentPrefab == null) return;

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
                    if (placedObject == null && placedPlant == null) CanPlace();
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

            switch (place.tag)
            {
                case "Tile":
                    DestroyImmediate(_hitObject.GetComponent<BaseGrid>().GetTile().gameObject);
                    _hitObject.GetComponent<BaseGrid>().PlaceTile(place.GetComponent<TileData>());
                    break;

                case "Object":
                    _hitObject.GetComponent<BaseGrid>().PlaceObject(place.GetComponent<ObjectData>());
                    break;

                case "Plant":   
                    _hitObject.GetComponent<BaseGrid>().PlacePlant(place.GetComponent<PlantData>());
                    break;
            }

            if (database.GetCountFromID(place.GetComponent<PlaceableObject>().GetID()) > 0)
            {
                database.PlaceData(place.GetComponent<PlaceableObject>().GetID());
                curButton.UpdateCountTMP(database.GetCountFromID(place.GetComponent<PlaceableObject>().GetID()));
            }
               
            if (database.GetCountFromID(place.GetComponent<PlaceableObject>().GetID()) == 0) CancelPlace();

            place.GetComponent<PlaceableObject>().SetPosition(_gridPos);
            place.transform.SetParent(objectParent);
        }
    }

    private void CancelPlace()
    {
        currentPrefab = null;
        DestroyImmediate(ghostObject.gameObject);
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

        currentPrefab = prefab;

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
        curButton = _button;
    }
}
