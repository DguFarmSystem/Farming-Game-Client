using System.Collections.Generic;
using UnityEngine;

public class JsonHelper
{
    public static List<T> FromJsonArray<T>(string json)
    {
        string wrapped = "{\"array\":" + json + "}";
        Wrapper<T> w = JsonUtility.FromJson<Wrapper<T>>(wrapped);
        return w.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> array;
    }
}
