// Unity
using UnityEngine;
using UnityEngine.UI;
using System;

[System.Serializable]
public class Database
{
    public string id;
    public string name;
    public PlaceableObject prefab;
    public Sprite sprite;

    public int count;
    public PlaceType type;
    public int price;
}

[CreateAssetMenu(fileName = "ObjectDatabase", menuName = "Data/ObjectDatabase")]
public class ObjectDatabase : ScriptableObject
{
    [SerializeField] private PlaceableObject[] prefabs;

    [SerializeField] private Database[] data;

    public PlaceableObject GetDataFromID(string id)
    {
        foreach (Database datum in data)
        {
            if (datum.id == id)
                return datum.prefab;
        }
        return null;
    }

    public string GetID(int index)
    {
        return data[index].id;
    }

    public string GetName(int index)
    {
        return data[index].name;
    }

    public Sprite GetSprite(int index)
    {
        return data[index].sprite;
    }

    public int GetCountFromIndex(int index)
    {
        return data[index].count;
    }

    public int GetCountFromID(string id)
    {
        foreach (Database datum in data)
        {
            if (datum.id == id)
            {
                return datum.count;
            }
        }

        return 0;
    }

    public void PlaceData(string id)
    {
        foreach (Database datum in data)
        {
            if (datum.id == id)
            {
                if (datum.count <= 0) return;
                datum.count--;
            }
        }
    }

    public void AddData(string id)
    {
        foreach (Database datum in data)
        {
            if (datum.id == id)
            {
                if (datum.count == -1) return;
                datum.count++;
            }
        }
    }

    public void DemolitionData(string id)
    {
        foreach (Database datum in data)
        {
            if (datum.id == id)
            {
                datum.count++;
            }
        }
    }

    public void SetCount(int index, int count)
    {
        data[index].count = count;
    }

    public PlaceType GetType(int index)
    {
        return data[index].type;
    }

    public int GetItemCount()
    {
        return data.Length;
    }

}
