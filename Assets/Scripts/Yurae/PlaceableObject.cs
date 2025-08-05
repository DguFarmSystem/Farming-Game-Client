// Unity
using UnityEngine;

public enum PlaceType
{
    Tile, Object, Plant
}

[DisallowMultipleComponent]
public class PlaceableObject : MonoBehaviour
{
    private Vector2Int gridPosition;

    [SerializeField] private string id;

    public string GetID()
    {
        return id;
    }

    private void OnEnable()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = GridManager.Instance.GetGridPosition(mousePos);

        transform.position = GridManager.Instance.GetWorldPosition(gridPos.x, gridPos.y);
    }

    public void SetPosition(Vector2Int gridPos)
    {
        gridPosition = gridPos;
        transform.position = GridManager.Instance.GetWorldPosition(gridPos.x, gridPos.y);
    }

    public Vector2Int GetPosition()
    {
        return gridPosition;
    }
}
