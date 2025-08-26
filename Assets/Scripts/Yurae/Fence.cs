// Unity
using System.Data;
using UnityEngine;

[System.Flags]
public enum FenceDirection
{
    None = 0,
    Left = 1 << 0,   // 0001
    Right = 1 << 1,   // 0010
    Up = 1 << 2,   // 0100
    Down = 1 << 3    // 1000
}

[DisallowMultipleComponent]
public class Fence : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;

    private PlaceableObject placeableObject;
    private SpriteRenderer spriteRenderer;

    private GridManager gridManager;

    private void Start()
    {
        placeableObject = GetComponent<PlaceableObject>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gridManager = FindFirstObjectByType<GridManager>();
    }

    private void Update()
    {
        UpdateSprite();
    }

    /// <summary>
    /// ���� ��ġ �������� ����� ���� �����ؼ� ��������Ʈ ����
    /// </summary>
    public void UpdateSprite()
    {
        if (placeableObject == null) return;
        Vector2Int pos = placeableObject.GetPosition();
        FenceDirection dir = GetConnections(pos);
        spriteRenderer.sprite = GetFenceSprite(dir);
    }

    /// <summary>
    /// ���� ĭ�� Fence�� �ִ��� üũ
    /// </summary>
    private FenceDirection GetConnections(Vector2Int pos)
    {
        FenceDirection dir = FenceDirection.None;

        if (gridManager.HasFenceAt(pos + Vector2Int.left)) dir |= FenceDirection.Left;
        if (gridManager.HasFenceAt(pos + Vector2Int.right)) dir |= FenceDirection.Right;
        if (gridManager.HasFenceAt(pos + Vector2Int.up)) dir |= FenceDirection.Up;
        if (gridManager.HasFenceAt(pos + Vector2Int.down)) dir |= FenceDirection.Down;

        return dir;
    }

    /// <summary>
    /// FenceDirection�� ���ڿ� Ű(L, R, U, D)�� ��ȯ�ؼ� �´� ��������Ʈ ã��
    /// </summary>
    private Sprite GetFenceSprite(FenceDirection dir)
    {
        string key = "";

        if (dir.HasFlag(FenceDirection.Left)) key += "L";
        if (dir.HasFlag(FenceDirection.Right)) key += "R";
        if (dir.HasFlag(FenceDirection.Up)) key += "U";
        if (dir.HasFlag(FenceDirection.Down)) key += "D";

        if (string.IsNullOrEmpty(key)) key = "M"; // ȥ�� ���� ��� ���(M)

        string spriteName = "Fence_Wood_" + key + "_0";
        Debug.Log(spriteName);

        foreach (Sprite sprite in sprites)
        {
            if (sprite.name == spriteName)
                return sprite;
        }

        Debug.LogWarning($"Fence sprite not found for key: {key}");
        return null;
    }
}
