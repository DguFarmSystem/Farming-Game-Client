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

    private void Update()
    {
        if (placementManager.IsCarrying()) return;

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
        flipButton.onClick.AddListener(()=>FlipObject());

        destroyButton.onClick.RemoveAllListeners();
        destroyButton.onClick.AddListener(() => DestroyObject(_grid));
    }

    public void DeslectObject()
    {
        placedObject = null;
        hit = null;
        uiPanel.SetActive(false);
    }

    public void DestroyObject(BaseGrid _grid)
    {
        if (placedObject == null) return;
        
        if (_grid.GetObject() != null)
        {
            PlaceableObject placeableObj = _grid.GetObject().GetComponent<PlaceableObject>();
            database.AddData(placeableObj.GetID());
            buildManager.Init();
            _grid.InitObject();
        }
        else if (_grid.GetPlant() != null)
        {
            PlaceableObject placeableObj = _grid.GetPlant().GetComponent<PlaceableObject>();
            database.AddData(placeableObj.GetID());
            buildManager.Init();
            _grid.InitPlant();
        }

        DestroyImmediate(placedObject);
        DeslectObject();
    }

    public void FlipObject()
    {
        if (placedObject == null) return;

        SpriteRenderer sprite = placedObject.GetComponent<SpriteRenderer>();
        sprite.flipX = !sprite.flipX;

        DeslectObject();
    }
}
