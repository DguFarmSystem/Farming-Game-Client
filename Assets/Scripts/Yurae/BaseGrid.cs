// Unity
using UnityEngine;

public class BaseGrid : MonoBehaviour
{
    [SerializeField] private Vector2Int grisPos;
    [SerializeField] private TileData placedTile;
    [SerializeField] private ObjectData placedObject;
    [SerializeField] private PlantData placedPlant;

    public void SetGridPos(Vector2Int _pos)
    {
        grisPos = _pos;
    }

    public Vector2Int GetGridPos()
    {
        return grisPos;
    }

    public void PlaceTile(TileData _tileData)
    {
        placedTile = _tileData;
    }

    public TileData GetTile()
    {
        return placedTile;
    }

    public void PlaceObject(ObjectData _objectData)
    {
        placedObject = _objectData;
    }

    public ObjectData GetObject()
    {
        return placedObject;
    }

    public void PlacePlant(PlantData _plantData)
    {
        placedPlant = _plantData;
    }

    public PlantData GetPlant()
    {
        return placedPlant;
    }
}
