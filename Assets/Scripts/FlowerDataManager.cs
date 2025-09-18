using System;
using System.Collections.Generic;
using UnityEngine;
// FlowerDataManager.cs 또는 별도 파일에 추가
[System.Serializable]
public class DexRegisterDto
{
    public long ownedPlant;
}

public class FlowerDataManager : MonoBehaviour
{
    public static FlowerDataManager Instance { get; private set; }

    public FlowerDataSO flowerData;
    private Dictionary<int, string> _dexIdToKo;

    [SerializeField] private ObjectDatabase objectDB;

    // 등급별 가중치 (노말:레어:에픽:레전드 = 20:10:4:1)
    [Header("등급 가중치")]
    public float weightNormal = 20f;
    public float weightRare = 10f;
    public float weightEpic = 4f;
    public float weightLegend = 1f;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        BuildNameCache();
    }

    private void BuildNameCache()
    {
        _dexIdToKo = new Dictionary<int, string>();
        if (flowerData == null || objectDB == null) return;

        foreach (var f in flowerData.flowerList)
        {
            if (f == null) continue;
            if (objectDB.TryGetIndexByStoreNo(f.dexId, out var idx))
            {
                var ko = objectDB.GetName(idx);
                if (!string.IsNullOrEmpty(ko))
                    _dexIdToKo[f.dexId] = ko;
            }
        }
    }

    public string GetDisplayName(string flowerName)
    {
        if (string.IsNullOrEmpty(flowerName) || flowerData == null) return flowerName;

        var f = flowerData.flowerList.Find(x => x != null && x.flowerName == flowerName);
        if (f == null) return flowerName;

        if (_dexIdToKo != null && _dexIdToKo.TryGetValue(f.dexId, out var ko))
            return ko;

        if (objectDB != null && objectDB.TryGetIndexByStoreNo(f.dexId, out var idx))
        {
            var nameKo = objectDB.GetName(idx);
            return string.IsNullOrEmpty(nameKo) ? flowerName : nameKo;
        }

        return flowerName;
    }

    //도감 등록
    public void RegisterFlower(string flowerName, long flower_servermapping)
    {
        foreach (var flower in flowerData.flowerList)
        {
            if (flower.flowerName == flowerName)
            {
                if (flower.isRegistered) return; // 이미 등록된 경우 중복 호출 방지

                flower.isRegistered = true;
                FindFirstObjectByType<BadgeManager>(FindObjectsInactive.Include)?.ReevaluateBadges();

                // 1. 요청에 필요한 DTO 객체 생성
                DexRegisterDto dto = new DexRegisterDto
                {
                    ownedPlant = flower_servermapping
                };

                // 2. DTO를 JSON 문자열로 변환
                string json = JsonUtility.ToJson(dto);

                // 3. APIManager를 사용하여 POST 요청 전송
                APIManager.Instance.Post(
                    "/api/dex",
                    json,
                    (response) =>
                    {
                        // 요청 성공 시 로직
                        Debug.Log("[Dex API] 등록 성공: " + response);
                    },
                    (error) =>
                    {
                        // 요청 실패 시 로직
                        Debug.LogError("[Dex API] 등록 실패: " + error);
                    }
                );

                return;
            }
        }

        Debug.LogWarning("등록하려는 꽃 없음: " + flowerName);
    }

    // 서버에서 받은 등록 상태 반영 (예: 서버가 등록된 flowerName 목록을 내려줌)
    public void ApplyRegistrationFromServer(IEnumerable<string> registeredFlowerNames)
    {
        var set = new HashSet<string>(registeredFlowerNames ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        foreach (var f in flowerData.flowerList)
        {
            if (f == null) continue;
            f.isRegistered = set.Contains(f.flowerName);
        }
    }


    //꽃이름을 받아와서 스프라이트를 가져오기
    public Sprite GetFlowerSprite(string flowerName)
    {
        var flower = flowerData.flowerList.Find(f => f.flowerName == flowerName);
        if (flower != null)
        {
            return flower.isRegistered ? flower.originalSprite : flower.silhouetteSprite;
        }
        return null;
    }

    public Sprite GetFlowerOriginalSprite(string flowerName)
    {
        var flower = flowerData.flowerList.Find(f => f.flowerName == flowerName);
        if (flower != null)
        {
            return flower.originalSprite;
        }
        return null;
    }

    //랜덤 꽃 이름 가져오기
    public string GetRandomFlowerName()
    {
        if (flowerData == null || flowerData.flowerList.Count == 0)
        {
            Debug.LogWarning("꽃 리스트가 비어 있습니다!");
            return null;
        }

        int idx = UnityEngine.Random.Range(0, flowerData.flowerList.Count);
        return flowerData.flowerList[idx].flowerName;
    }

    // 등급 가중치 랜덤으로 하나 뽑기 (중복 허용, 등급 내 균등)
    public string GetRandomFlowerNameByRarityWeighted()
    {
        if (flowerData == null || flowerData.flowerList == null || flowerData.flowerList.Count == 0)
        {
            Debug.LogWarning("꽃 리스트가 비어 있습니다!");
            return null;
        }

        // 등급별 버킷 구성
        var bucketNormal = new List<FlowerData>();
        var bucketRare = new List<FlowerData>();
        var bucketEpic = new List<FlowerData>();
        var bucketLegend = new List<FlowerData>();

        foreach (var f in flowerData.flowerList)
        {
            if (f == null) continue;
            switch (f.rarity)
            {
                case FlowerRarity.Normal: bucketNormal.Add(f); break;
                case FlowerRarity.Rare: bucketRare.Add(f); break;
                case FlowerRarity.Epic: bucketEpic.Add(f); break;
                case FlowerRarity.Legend: bucketLegend.Add(f); break;
            }
        }

        // 비어 있는 등급은 가중치 0 취급
        float wN = bucketNormal.Count > 0 ? weightNormal * bucketNormal.Count : 0f;
        float wR = bucketRare.Count > 0 ? weightRare * bucketRare.Count : 0f;
        float wE = bucketEpic.Count > 0 ? weightEpic * bucketEpic.Count : 0f;
        float wL = bucketLegend.Count > 0 ? weightLegend * bucketLegend.Count : 0f;

        float totalW = wN + wR + wE + wL;
        if (totalW <= 0f)
        {
            // 모든 버킷이 비어있으면 전체에서 임의 선택
            var any = flowerData.flowerList[UnityEngine.Random.Range(0, flowerData.flowerList.Count)];
            return any.flowerName;
        }

        // 등급 선택 (룰렛)
        float pick = UnityEngine.Random.value * totalW;
        List<FlowerData> chosenBucket;
        if (pick < wN) chosenBucket = bucketNormal;
        else if ((pick -= wN) < wR) chosenBucket = bucketRare;
        else if ((pick -= wR) < wE) chosenBucket = bucketEpic;
        else chosenBucket = bucketLegend;

        // 등급 내 균등 랜덤
        var chosen = chosenBucket[UnityEngine.Random.Range(0, chosenBucket.Count)];
        return chosen.flowerName;
    }

    public String Get_Rarity(string flower_name)
    {
        var flower = flowerData.flowerList.Find(f => f.flowerName == flower_name);
        if (flower != null)
        {
            return flower.rarity.ToString(); // "Normal", "Rare" ...
        }
        else
        {
            Debug.LogWarning("등급을 찾을 꽃이 없습니다: " + flower_name);
            return null;
        }
    }
}
