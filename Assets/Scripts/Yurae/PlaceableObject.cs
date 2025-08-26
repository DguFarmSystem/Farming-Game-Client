// Unity
using UnityEngine;

public enum PlaceType
{
    Tile, Object, Plant
}

public enum RotationType
{
    
}

[DisallowMultipleComponent]
public class PlaceableObject : MonoBehaviour
{
    [Header("Postion (Grid)")]
    [SerializeField] private Vector2Int gridPosition;

    [Header("ID")]
    [SerializeField] private string id;

    [Header("Rotation Sprite")]
    [SerializeField] private Sprite[] rotationSprites;
    private int spriteIndex; // => Modify Rotation Enum

    private SpriteRenderer spriteRenderer;

    public string GetID()
    {
        return id;
    }

    private void OnEnable()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = GridManager.Instance.GetGridPosition(mousePos);

        transform.position = GridManager.Instance.GetWorldPosition(gridPos.x, gridPos.y);

        spriteRenderer = GetComponent<SpriteRenderer>();
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

    /// <summary>
    /// Rotation Method 
    /// </summary>
    public void Rotation()
    {
        if (rotationSprites.Length == 0)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            return;
        }

        spriteIndex++;
        spriteIndex = spriteIndex % rotationSprites.Length;

        spriteRenderer.sprite = rotationSprites[spriteIndex];
    }
}
