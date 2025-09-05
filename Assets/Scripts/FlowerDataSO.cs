using System;
using System.Collections.Generic;
using UnityEngine;


public enum FlowerRarity { Normal, Rare, Epic, Legend }

[System.Serializable]
public class FlowerData
{
    public string flowerName;
    public Sprite silhouetteSprite;
    public Sprite originalSprite;
    public int dexId;

    public FlowerRarity rarity = FlowerRarity.Normal;
    [Min(0)] public float weightOverride = -1f; 
    // -1: 사용 안 함(등급 기본 가중치 사용), 0 이상: 이 꽃만의 가중치로 덮어쓰기

    [NonSerialized] public bool isRegistered = false;  // 런타임에서만 바뀜!
}

[CreateAssetMenu(fileName = "FlowerDexSO", menuName = "Scriptable Objects/FlowerDexSO")]
public class FlowerDataSO : ScriptableObject
{
    public List<FlowerData> flowerList;


    [Header("한 번에 채울 오리지널 스프라이트 리스트(순서대로 채움)")]
    public List<Sprite> originalSprites = new();

    [Tooltip("true면 Sprite.name과 flowerName을 매칭, false면 인덱스 순서대로 매칭")]
    public bool matchByName = false;


    // 에디터에서 값이 바뀌거나 저장될 때 자동 호출됨
    private void OnValidate()
    {
        ApplyOriginalSprites();
    }

    // 우클릭 메뉴로 수동 실행도 가능
    [ContextMenu("Apply Original Sprites")]
    public void ApplyOriginalSprites()
    {
        if (flowerList == null || flowerList.Count == 0) return;
        if (originalSprites == null || originalSprites.Count == 0) return;

        if (matchByName)
            ApplyByName();
        else
            ApplyByIndex();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void ApplyByIndex()
    {
        int n = Mathf.Min(flowerList.Count, originalSprites.Count);
        for (int i = 0; i < n; i++)
        {
            flowerList[i].originalSprite = originalSprites[i];
        }
    }

    private void ApplyByName()
    {
        // 스프라이트 이름 → 스프라이트 캐시
        var dict = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        foreach (var sp in originalSprites)
        {
            if (sp == null) continue;
            if (!dict.ContainsKey(sp.name)) dict.Add(sp.name, sp);
        }

        foreach (var f in flowerList)
        {
            if (f == null || string.IsNullOrEmpty(f.flowerName)) continue;
            // 정확히 이름이 같은 스프라이트가 있으면 매칭
            if (dict.TryGetValue(f.flowerName, out var sp))
            {
                f.originalSprite = sp;
            }
        }
    }
}



