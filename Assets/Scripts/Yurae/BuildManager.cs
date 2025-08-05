// Unity
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BuildManager : MonoBehaviour
{
    public PlacementManager placementManager;
    public GameObject[] placeablePrefabs;

    [SerializeField] private ObjectDatabase database;
    [SerializeField] private GameObject objectSelectButton;
    [SerializeField] private Transform[] parents;

    private readonly int tileParentId = 0;
    private readonly int objectParentId = 1;
    private readonly int plantParentId = 2;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        for (int i = 0; i < placeablePrefabs.Length; i++)
        {
            GameObject obj = Instantiate(objectSelectButton);

            Button button = obj.GetComponent<Button>();

            int index = i;
            button.onClick.AddListener(() => OnClickPlace(index));

            ObjectSelectButton objSelectButton = obj.GetComponent<ObjectSelectButton>();
            button.onClick.AddListener(() => SetCurrentButton(objSelectButton));

            string name = database.GetName(index);
            Sprite sprite = database.GetSprite(index);
            int count = database.GetCountFromIndex(index);

            objSelectButton.Init(name, sprite, count);

            PlaceType type = database.GetType(index);

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
        placementManager.SetPrefabToPlace(placeablePrefabs[index]);
    }

    public void SetCurrentButton(ObjectSelectButton _button)
    {
        placementManager.SetCurrentButton(_button);
    }
}
