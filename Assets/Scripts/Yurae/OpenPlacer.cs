using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class OpenPlacer : MonoBehaviour
{
    [SerializeField] private BuildManager build;

    private Button placer;

    private void Start()
    {
        placer = GetComponent<Button>();
        placer.onClick.AddListener(build.Init);
    }
}
