// Unity
using UnityEngine;
using UnityEngine.UI;

// System
using System.Collections.Generic;

[DisallowMultipleComponent]
public class BuildManager : MonoBehaviour
{
    public PlacementManager placementManager;
    public GameObject[] placeablePrefabs;

    [SerializeField] private ObjectDatabase database;
    [SerializeField] private GameObject objectSelectButton;
    [SerializeField] private Transform[] parents;
    [SerializeField] private List<ObjectSelectButton> objectSelectButtons;

    private readonly int tileParentId = 0;
    private readonly int objectParentId = 1;
    private readonly int plantParentId = 2;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        foreach(ObjectSelectButton button in objectSelectButtons)
        {
            DestroyImmediate(button.gameObject);
        }

        objectSelectButtons.Clear();

        for (int i = 0; i < placeablePrefabs.Length; i++)
        {
            int index = i;

            if (database.GetCountFromIndex(index) == 0) continue;

            GameObject obj = Instantiate(objectSelectButton);

            Button button = obj.GetComponent<Button>();

            button.onClick.AddListener(() => OnClickPlace(index));

            ObjectSelectButton objSelectButton = obj.GetComponent<ObjectSelectButton>();
            objectSelectButtons.Add(objSelectButton);
            button.onClick.AddListener(() => SetCurrentButton(objSelectButton));

            string name = database.GetName(index);
            Sprite sprite = database.GetSprite(index);
            int count = database.GetCountFromIndex(index);

            PlaceType type = database.GetType(index);

            objSelectButton.Init(placeablePrefabs[i].GetComponent<PlaceableObject>().GetID(), name, sprite, count, type);

            switch (type)
            {
                case PlaceType.Tile:
                    objSelectButton.transform.SetParent(parents[tileParentId]);
                    break;

                case PlaceType.Object:
                    objSelectButton.transform.SetParent(parents[objectParentId]);
                    break;

                case PlaceType.Plant:
                    objSelectButton.transform.SetParent(parents[plantParentId]);
                    break;

                default:
                    break;
            }

            objSelectButton.transform.localScale = Vector3.one;
        }
    }

    public void OnClickPlace(int index)
    {
        placementManager.SetPrefabToPlace(placeablePrefabs[index].gameObject);
    }

    public void SetCurrentButton(ObjectSelectButton _button)
    {
        placementManager.SetCurrentButton(_button);
    }

    public void UpdateCountTMP()
    {
        Debug.Log($"[Build] DB instance = {database?.GetInstanceID()}");

        foreach (ObjectSelectButton button in objectSelectButtons)
        {
            button.UpdateCountTMPFromDatabse();
        }
    }

    void OnEnable()
    {
        UpdateCountTMP();
    }
}
