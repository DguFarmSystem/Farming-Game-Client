// Unity
using UnityEngine;

public enum PlantType
{
    Land, Water
}

public class PlantData : MonoBehaviour
{
    [SerializeField] private PlantType type;

    public PlantType Type()
    {
        return type;
    }
}
