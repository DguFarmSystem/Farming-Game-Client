using UnityEngine;

public class ResourceManager
{
    public T Load<T>(string _folder, string _fileName) where T : UnityEngine.Object
    {
        string _path = $"{_folder}/{_fileName}";

        T resource = Resources.Load<T>(_path);

        if (resource == null) Debug.LogError("Error! Can't Find Resource");
        return resource;
    }
}
