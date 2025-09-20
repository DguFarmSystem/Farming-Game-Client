// Unity
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TileSelectionUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button flipButton;
    [SerializeField] private Button destroyButton;

    [Header("Settings")]
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private GameObject uiPanel;
    [SerializeField] GardeenObjectPanel gardenObjectPanel;
    [SerializeField] private ObjectDatabase database;
    [SerializeField] private BuildManager buildManager;
    [SerializeField] private PlacementManager placementManager;

    [Header("Debug")]
    [SerializeField] private GameObject placedObject;
    [SerializeField] private Collider2D hit;

    private CameraMovement cameraMovement;

    public bool IsSelected;

    private void Start()
    {
        cameraMovement = Camera.main.GetComponent<CameraMovement>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            DeslectObject();
        }

        if (placementManager.IsCarrying()) return;

        if (IsSelected) return;

        if (Input.GetMouseButtonDown(0) && gardenObjectPanel.IsPlaceMode())
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.CompareTag("Base Grid"))
            {
                BaseGrid grid = hit.GetComponent<BaseGrid>();
                SelectObject(grid);
            }
        }
    }

    private void SelectObject(BaseGrid _grid)
    {
        if (_grid.GetObject() == null && _grid.GetPlant() == null) return;

        cameraMovement.CanMove = false;
        IsSelected = true;

        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, _grid.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            Camera.main,
            out Vector2 localPoint
        );

        if (_grid.GetObject() != null)
        {
            placedObject = _grid.GetObject().gameObject;
        }
        else if (_grid.GetPlant() != null)
        {
            placedObject = _grid.GetPlant().gameObject;
        }

        uiPanel.SetActive(true);
        uiPanel.transform.localPosition = localPoint;

        flipButton.onClick.RemoveAllListeners();
        flipButton.onClick.AddListener(()=>RotationObject());

        destroyButton.onClick.RemoveAllListeners();
        destroyButton.onClick.AddListener(() => DestroyObject(_grid));
    }

    public void DeslectObject()
    {
        placedObject = null;
        hit = null;
        uiPanel.SetActive(false);
        IsSelected = false;

        cameraMovement.CanMove = true;
    }

    public void DestroyObject(BaseGrid _grid)
    {
        if (placedObject == null) return;
        
        if (_grid.GetObject() != null)
        {
            PlaceableObject placeableObj = _grid.GetObject().GetComponent<PlaceableObject>();
            //database.AddData(placeableObj.GetID());

            database.ChangeCountByID(placeableObj.GetID(), 1,
            onSuccess: () =>
            {
                buildManager.Init();
                _grid.InitObject();
            },
            onError: e => Debug.LogError(e));
        }
        else if (_grid.GetPlant() != null)
        {
            PlaceableObject placeableObj = _grid.GetPlant().GetComponent<PlaceableObject>();
            //database.AddData(placeableObj.GetID());

            database.ChangeCountByID(placeableObj.GetID(), 1,
            onSuccess: () =>
            {
                buildManager.Init();
                _grid.InitPlant();
            },
            onError: e => Debug.LogError(e));
        }

        GardenControllerAPI.ClearGardenObject(
            _grid.GetGridPos().x, _grid.GetGridPos().y, tileType: _grid.GetTile().GetComponent<PlaceableObject>().GetNoID(),
            onSuccess: res => Debug.Log("Object Remove!"),
            onError: err => Debug.LogError(err)
        );

        DestroyImmediate(placedObject);
        DeslectObject();
    }

    public void RotationObject()
    {
        if (placedObject == null) return;

        PlaceableObject placeableObject = placedObject.GetComponent<PlaceableObject>();
        placeableObject.Rotation();

        DeslectObject();
    }
}
