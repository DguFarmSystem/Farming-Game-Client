// Unity
using UnityEngine;

// System
using System;
using System.Collections.Generic;

[Serializable]
public class Database
{
    [Header("ID/Name/Prefab/Sprite")]
    public string name;
    public string id;
    public PlaceableObject prefab;
    public Sprite sprite;

    [Header("Gameplay")]
    public int count;
    public PlaceType type;
    public int price;

    [Header("Server Mapping")]
    public long storeGoodsNumber;
}

[CreateAssetMenu(fileName = "ObjectDatabase", menuName = "Data/ObjectDatabase")]
public class ObjectDatabase : ScriptableObject
{
    [SerializeField] private Database[] data;

    private Dictionary<string, int> _id2idx;
    private Dictionary<long, int> _storeNo2idx;

    public PlaceableObject GetDataFromID(string id)
    {
        if (string.IsNullOrEmpty(id) || data == null) return null;
        if (_id2idx == null) BuildIndexMaps();
        if (_id2idx.TryGetValue(id, out var i))
            return (i >= 0 && i < data.Length) ? data[i].prefab : null;
        return null;
    }

    public string GetID(int index) => (data != null && InRange(index)) ? data[index].id : null;
    public string GetName(int index) => (data != null && InRange(index)) ? data[index].name : null;
    public Sprite GetSprite(int index) => (data != null && InRange(index)) ? data[index].sprite : null;
    public int GetCountFromIndex(int index) => (data != null && InRange(index)) ? data[index].count : 0;

    public string GetNameFromID(string id)
    {
        foreach (Database datum in data)
        {
            if (datum.id == id)
            {
                Debug.Log(datum.id);
                return datum.name;
            }

        }
        return null;
    }

    public Sprite GetSpriteFromID(string id)
    {
        foreach (Database datum in data)
        {
            if (datum.id == id)
                return datum.sprite;
        }
        return null;
    }

    public int GetCountFromID(string id)
    {
        if (string.IsNullOrEmpty(id) || data == null) return 0;
        if (_id2idx == null) BuildIndexMaps();
        return _id2idx.TryGetValue(id, out var i) ? data[i].count : 0;
    }

    public void PlaceData(string id)
    {
        if (string.IsNullOrEmpty(id) || data == null) return;
        if (_id2idx == null) BuildIndexMaps();
        if (_id2idx.TryGetValue(id, out var i))
        {
            if (data[i].count > 0) data[i].count--;
        }
    }

    public void AddData(string id)
    {
        if (string.IsNullOrEmpty(id) || data == null) return;
        if (_id2idx == null) BuildIndexMaps();
        if (_id2idx.TryGetValue(id, out var i))
        {
            if (data[i].count != -1) data[i].count++;
        }
    }

    public void SetCount(int index, int count)
    {
        if (data == null || !InRange(index)) return;
        data[index].count = Mathf.Max(0, count);
    }

    public PlaceType GetType(int index) => (data != null && InRange(index)) ? data[index].type : default;
    public int GetItemCount() => data != null ? data.Length : 0;

    public long GetStoreGoodsNumber(int index) => (data != null && InRange(index)) ? data[index].storeGoodsNumber : -1;
    public bool TryGetIndexByID(string id, out int index)
    {
        index = -1;
        if (string.IsNullOrEmpty(id) || data == null) return false;
        if (_id2idx == null) BuildIndexMaps();
        return _id2idx.TryGetValue(id, out index);
    }
    public bool TryGetIndexByStoreNo(long storeNo, out int index)
    {
        index = -1;
        if (data == null) return false;
        if (_storeNo2idx == null) BuildIndexMaps();
        return _storeNo2idx.TryGetValue(storeNo, out index);
    }

    [Serializable] private class ServerInvRow { public long object_type; public int object_count; }
    [Serializable] private class ServerInvEnvelope { public int status; public string message; public ServerInvRow[] data; }

    public void ApplyInventoryJson(string json, bool zeroMissing = true)
    {
        if (string.IsNullOrEmpty(json) || data == null) return;

        ServerInvEnvelope env = null;
        try { env = JsonUtility.FromJson<ServerInvEnvelope>(json); }
        catch (Exception ex)
        {
            Debug.LogError($"[ObjectDB] ???? ?? ??: {ex.Message}\n???: {json.Substring(0, Mathf.Min(300, json.Length))}");
            return;
        }

        var rows = env?.data;
        if (rows == null)
        {
            Debug.LogWarning("[ObjectDB] ???? data? ?????.");
            if (zeroMissing) ResetAllCountsToZero();
            return;
        }

        ApplyInventoryRows(rows, zeroMissing);
    }

    public void ApplyInventoryRows(IEnumerable<object> anyRows, bool zeroMissing = true)
    {
        if (anyRows == null || data == null) return;
        var casted = new List<ServerInvRow>();
        foreach (var r in anyRows)
        {
            if (r is ServerInvRow sr) casted.Add(sr);
        }
        ApplyInventoryRows(casted, zeroMissing);
    }

    private void ApplyInventoryRows(IEnumerable<ServerInvRow> rows, bool zeroMissing)
    {
        if (_storeNo2idx == null) BuildIndexMaps();

        if (zeroMissing)
            ResetAllCountsToZero();

        int applied = 0, unknown = 0;
        foreach (var r in rows)
        {
            if (TryGetIndexByStoreNo(r.object_type, out var idx))
            {
                data[idx].count = Mathf.Max(0, r.object_count);
                applied++;
            }
            else
            {
                unknown++;
                Debug.LogWarning($"[ObjectDB] ???? ?? object_type={r.object_type} (storeGoodsNumber ????)");
            }
        }
        Debug.Log($"[ObjectDB] ???? ?? ??: ?? {applied}?, ??? {unknown}?");
    }

    public void ResetAllCountsToZero()
    {
        if (data == null) return;
        for (int i = 0; i < data.Length; i++) data[i].count = 0;
    }

    private bool InRange(int index) => index >= 0 && index < (data?.Length ?? 0);

    private void BuildIndexMaps()
    {
        _id2idx = new Dictionary<string, int>(StringComparer.Ordinal);
        _storeNo2idx = new Dictionary<long, int>();

        if (data == null) return;
        for (int i = 0; i < data.Length; i++)
        {
            var d = data[i];
            if (!string.IsNullOrEmpty(d.id))
            {
                if (!_id2idx.ContainsKey(d.id)) _id2idx.Add(d.id, i);
                else Debug.LogWarning($"[ObjectDB] ?? id ??: {d.id} (idx {i})");
            }
            if (d.storeGoodsNumber != 0) // 0? ????? ??
            {
                if (!_storeNo2idx.ContainsKey(d.storeGoodsNumber)) _storeNo2idx.Add(d.storeGoodsNumber, i);
                else Debug.LogWarning($"[ObjectDB] ?? storeGoodsNumber ??: {d.storeGoodsNumber} (idx {i})");
            }
        }
    }

    private void OnEnable() => BuildIndexMaps();
}
