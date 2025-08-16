// ShopPurchaseBridge.cs
using UnityEngine;
using System.Collections;

public class ShopPurchaseBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectDatabase objectDatabase;
    [SerializeField] private BagManager bagManager;

    [Header("Garden Lookup (runtime)")]
    [SerializeField] private string buildManagerTag = "BuildManager";
    [SerializeField] private string buildManagerObjectName = "Build UI Manager";

    private BuildManager cachedBuild;

    [Header("Options")]
    [SerializeField] private bool refreshBagOnPurchase = true;
    [SerializeField] private bool refreshGardenOnPurchase = true;

    public void OnPurchased(string resourceKey, string _displayName, int count)
    {
        if (string.IsNullOrEmpty(resourceKey) || objectDatabase == null || count <= 0) return;

        for (int i = 0; i < count; i++) objectDatabase.AddData(resourceKey);

        var bag = bagManager != null ? bagManager : BagManager.Instance;
        if (refreshBagOnPurchase && bag != null && bag.gameObject.activeInHierarchy)
            bag.Rebuild();

        if (refreshGardenOnPurchase) ForceRefreshGarden();
    }

    private BuildManager GetBuildManager()
    {
        if (cachedBuild != null && cachedBuild.gameObject) return cachedBuild;

        if (!string.IsNullOrEmpty(buildManagerTag))
        {
            var go = GameObject.FindWithTag(buildManagerTag);
            if (go) cachedBuild = go.GetComponent<BuildManager>();
            if (cachedBuild != null) return cachedBuild;
        }

        if (!string.IsNullOrEmpty(buildManagerObjectName))
        {
            var go = GameObject.Find(buildManagerObjectName);
            if (go) cachedBuild = go.GetComponent<BuildManager>();
            if (cachedBuild != null) return cachedBuild;
        }

        cachedBuild = FindFirstObjectByType<BuildManager>(FindObjectsInactive.Include);
        return cachedBuild;
    }

    private void ForceRefreshGarden()
    {
        var build = GetBuildManager();

        if (build != null)
        {
            try { build.UpdateCountTMP(); } catch { }
        }

        StartCoroutine(RefreshGardenNextFrame(build));
    }

    private IEnumerator RefreshGardenNextFrame(BuildManager build)
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        if (build != null)
        {
            try { build.UpdateCountTMP(); } catch { }
        }
    }
}
