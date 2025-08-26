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
    /// 현재 위치 기준으로 연결된 방향 조사해서 스프라이트 갱신
    /// </summary>
    public void UpdateSprite()
    {
        if (placeableObject == null) return;
        Vector2Int pos = placeableObject.GetPosition();
        FenceDirection dir = GetConnections(pos);
        spriteRenderer.sprite = GetFenceSprite(dir);
    }

    /// <summary>
    /// 인접 칸에 Fence가 있는지 체크
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
    /// FenceDirection을 문자열 키(L, R, U, D)로 변환해서 맞는 스프라이트 찾기
    /// </summary>
    private Sprite GetFenceSprite(FenceDirection dir)
    {
        string key = "";

        if (dir.HasFlag(FenceDirection.Left)) key += "L";
        if (dir.HasFlag(FenceDirection.Right)) key += "R";
        if (dir.HasFlag(FenceDirection.Up)) key += "U";
        if (dir.HasFlag(FenceDirection.Down)) key += "D";

        if (string.IsNullOrEmpty(key)) key = "M"; // 혼자 있을 경우 기둥(M)

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
