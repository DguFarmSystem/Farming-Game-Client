using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Garden
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RotationEnum { R0, R90, R180, R270 }

    [System.Serializable]
    public class ObjectData
    {
        public long objectType;
        public RotationEnum rotation;
    }

    [System.Serializable]
    public class ObjectLoadData
    {
        public long objectKind;
        public RotationEnum rotation;
    }

    [System.Serializable]
    public class GardenLoadData
    {
        public long tileId;
        public int x;
        public int y;
        public long tileType;

        [JsonProperty("object", NullValueHandling = NullValueHandling.Include)]
        public ObjectLoadData loadData;
    }

    [System.Serializable]
    public class GardenData
    {
        public long tileId;
        public int x;
        public int y;
        public long tileType;

        [JsonProperty("object")] 
        public ObjectData objectData;
    }

    [System.Serializable]
    public class ApiResponse<T>
    {
        public long status;
        public string message;
        public T data;
    }

    [System.Serializable]
    public class GardenUpdateRequest
    {
        public long tileType;

        [JsonProperty("object", NullValueHandling = NullValueHandling.Include)]
        public ObjectData objectData;
    }
}
