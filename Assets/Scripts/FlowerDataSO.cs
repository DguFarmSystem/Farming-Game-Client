using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FlowerData
{
    public string flowerName;
    public Sprite silhouetteSprite;
    public Sprite originalSprite;

    [NonSerialized] public bool isRegistered = false;  // 런타임에서만 바뀜!
}

[CreateAssetMenu(fileName = "FlowerDexSO", menuName = "Scriptable Objects/FlowerDexSO")]
public class FlowerDataSO : ScriptableObject
{
    public List<FlowerData> flowerList;
}
