using UnityEngine;
using System;
using System.Collections;

public class ShopPurchaseBridge : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ObjectDatabase objectDatabase;
    [SerializeField] private BagManager bagManager;

    [Header("API")]
    [SerializeField] private string inventoryGetEndpoint = "/api/inventory";
    [SerializeField] private string inventoryUpdateEndpoint = "/api/inventory/update";

    [Header("UI Refresh Options")]
    [SerializeField] private bool refreshBagOnPurchase = true;
    [SerializeField] private bool refreshGardenOnPurchase = true;

    [Header("Grid Upgrade Item")]
    [SerializeField] private long gridUpgradeResourceKey = 400050;
    [SerializeField] private int gridUpgradeMaxCalls = 3;
    private string PrefsKey => $"upgrade_calls_{gridUpgradeResourceKey}";

    public void OnPurchased(long resourceKey, string _displayName, int count)
    {
        if (resourceKey <= 0 || count <= 0) return;

        // 1) 업그레이드 아이템
        if (gridUpgradeResourceKey > 0 && resourceKey == gridUpgradeResourceKey)
        {
            ApplyGridUpgrade(count);
            return;
        }

        // 2) 일반 아이템 인벤토리 반영
        if (objectDatabase != null)
        {
            for (int i = 0; i < count; i++) objectDatabase.AddData(resourceKey.ToString());
        }
        else
        {
            Debug.LogWarning("[Shop] ObjectDatabase 미연결(일반 아이템)");
        }

        // 서버에 최종 수량 PUT
        StartCoroutine(PurchaseFlow(resourceKey, count));

        // 3) UI 갱신
        if (refreshBagOnPurchase)
        {
            var bag = bagManager != null ? bagManager : BagManager.Instance;
            if (bag && bag.gameObject.activeInHierarchy) bag.Rebuild();
        }
        if (refreshGardenOnPurchase)
        {
            var build = UnityEngine.Object.FindFirstObjectByType<BuildManager>(FindObjectsInactive.Include);
            if (build) build.UpdateCountTMP();
        }

    }

    private void ApplyGridUpgrade(int count)
    {
        int max = (gridUpgradeMaxCalls <= 0) ? 3 : gridUpgradeMaxCalls;

        const string LEGACY = "grid_upgrade_calls";
        if (PlayerPrefs.HasKey(LEGACY) && !PlayerPrefs.HasKey(PrefsKey))
        {
            int legacy = Mathf.Clamp(PlayerPrefs.GetInt(LEGACY, 0), 0, max);
            PlayerPrefs.SetInt(PrefsKey, legacy);
            PlayerPrefs.DeleteKey(LEGACY);
            PlayerPrefs.Save();
        }

        int used = Mathf.Clamp(PlayerPrefs.GetInt(PrefsKey, 0), 0, max);
        if (used >= max)
        {
            ShopItemSlot.RaiseUpgradeProgressChanged();
            Debug.Log("[Shop] GridUpgrade 이미 MAX");
            GameManager.Sound?.SFXPlay("SFX_ButtonCancle");
            return;
        }

        used += 1;
        PlayerPrefs.SetInt(PrefsKey, used);
        PlayerPrefs.Save();

        ShopItemSlot.RaiseUpgradeProgressChanged();
        Debug.Log($"[Shop] GridUpgrade {used}/{max}");

        if (GridManager.Instance) GridManager.Instance.LevelUp();
    }

    [Serializable] private class InvUpdateReq { public long object_type; public int object_count; }
    [Serializable] private class InvGetRow { public long object_type; public int object_count; }
    [Serializable] private class InvGetEnv { public int status; public string message; public InvGetRow[] data; }

    private IEnumerator PurchaseFlow(long resourceKey, int addCount)
    {
        if (resourceKey <= 0 || addCount <= 0) yield break;
        if (APIManager.Instance == null) yield break;

        long storeGoodsNumber = resourceKey;

        // 1) 서버 현재 수량 조회
        int serverCount = -1;
        yield return StartCoroutine(GetServerCount(storeGoodsNumber, v => serverCount = v));
        if (serverCount < 0)
        {
            Debug.LogError("[Buy] preflight GET failed");
            yield break;
        }

        // 2) 최종 수량 계산 및 PUT
        int finalCount = serverCount + addCount;
        var payload = new InvUpdateReq { object_type = storeGoodsNumber, object_count = finalCount };
        string json = JsonUtility.ToJson(payload);
        Debug.Log($"[Buy] FINAL PUT {inventoryUpdateEndpoint} body={json} (store_goods_number={storeGoodsNumber}, serverBefore={serverCount}, add={addCount}, final={finalCount})");

        bool postDone = false, postOk = false;
        APIManager.Instance.Put(
            inventoryUpdateEndpoint,
            json,
            ok => { postOk = true; Debug.Log($"[Buy] PUT OK: {ok}"); postDone = true; },
            err => { Debug.LogError($"[Buy] PUT ERR: {err}"); postDone = true; }
        );
        yield return new WaitUntil(() => postDone);
        if (!postOk) yield break;

        // 3) 서버 수량 재확인 및 가방 새로고침
        int after = -1;
        yield return StartCoroutine(GetServerCount(storeGoodsNumber, v => after = v));
        Debug.Log($"[Buy] verify serverCount={after}, expected={finalCount}");

        BagManager.Instance?.FetchInventoryAndRebuild();
        
    }

    private IEnumerator GetServerCount(long storeGoodsNumber, Action<int> onDone)
    {
        bool doneFlag = false;
        int val = 0;                 // 기본값을 0으로 (없으면 0개부터 시작)
        bool hardFail = false;       // 네트워크/파싱 진짜 실패만 구분

        APIManager.Instance.Get(
            inventoryGetEndpoint,
            ok =>
            {
                try
                {
                // 원문 프리뷰 남기기 (디버깅)
                string preview = ok.Length > 400 ? ok.Substring(0, 400) + "...(truncated)" : ok;
                    Debug.Log($"[Buy] GET ok raw preview: {preview}");

                // 기존 스키마 (data: [{object_type, object_count}])
                var env = JsonUtility.FromJson<InvGetEnv>(ok);
                    if (env?.data != null && env.data.Length > 0)
                    {
                        bool found = false;
                        foreach (var r in env.data)
                        {
                            if (r.object_type == storeGoodsNumber)
                            {
                                val = r.object_count;
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                        // 항목이 없으면 0개로 시작 (서버가 미생성 상태인 것)
                        val = 0;
                            Debug.Log($"[Buy] inventory missing key={storeGoodsNumber}, defaulting to 0");
                        }
                    }
                    else
                    {
                    // 전체가 빈 배열이면 전부 0개로 간주
                    val = 0;
                        Debug.Log("[Buy] inventory empty list, defaulting to 0 for all");
                    }
                }
                catch (Exception e)
                {
                    hardFail = true;     // 파싱 폭발 같은 진짜 실패만 하드 실패
                Debug.LogError($"[Buy] GET parse error: {e}");
                }
                finally { doneFlag = true; }
            },
            err =>
            {
                hardFail = true;         // 네트워크/401/5xx 등은 하드 실패
            Debug.LogError("[Buy] GET inventory error: " + err);
                doneFlag = true;
            }
        );

        yield return new WaitUntil(() => doneFlag);
        onDone?.Invoke(hardFail ? -1 : val);   // 하드 실패만 -1, 아니면 0 이상
    }

}
