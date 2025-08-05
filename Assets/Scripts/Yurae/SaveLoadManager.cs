// Unity
using UnityEngine;

// System
using System.Collections.Generic;

[System.Serializable]
public class ObjectSaveData
{
    public string prefabName;
    public Vector2Int position;
}

public class SaveLoadManager : MonoBehaviour
{
    public ObjectDatabase database;

    public void Save()
    {
        var dataList = new List<ObjectSaveData>();
        PlaceableObject[] objects = FindObjectsOfType<PlaceableObject>();

        foreach (var obj in objects)
        {
            dataList.Add(new ObjectSaveData
            {
                prefabName = obj.name.Replace("(Clone)", "").Trim(),
                position = obj.GetPosition()
            });
        }

        string json = JsonUtility.ToJson(new SerializationWrapper<ObjectSaveData>(dataList));
        PlayerPrefs.SetString("SaveData2D", json);
    }

    public void Load()
    {
        string json = PlayerPrefs.GetString("SaveData2D", "");
        if (string.IsNullOrEmpty(json)) return;

        var loaded = JsonUtility.FromJson<SerializationWrapper<ObjectSaveData>>(json);

        foreach (var data in loaded.items)
        {
            GameObject prefab = database.GetDataFromID(data.prefabName).gameObject;
            GameObject obj = Instantiate(prefab);
            obj.GetComponent<PlaceableObject>().SetPosition(data.position);
        }
    }
}

[System.Serializable]
public class SerializationWrapper<T>
{
    public List<T> items;
    public SerializationWrapper(List<T> list) => items = list;
}
