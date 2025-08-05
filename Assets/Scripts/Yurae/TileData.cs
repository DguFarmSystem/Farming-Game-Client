// Using
using UnityEngine;
public enum TileType
{
    Grass, Field, Water, Stone
}

public class TileData : MonoBehaviour
{
    [SerializeField] private string tileName;
    [SerializeField] private TileType tileType;

    public TileType Type()
    {
        return tileType;
    }

    public string GetName()
    {
        return tileName;
    }
}
