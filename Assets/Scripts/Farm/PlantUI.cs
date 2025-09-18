using UnityEngine;
using UnityEngine.UI;

public class PlantUI : MonoBehaviour
{
    public GameObject root; //유아이 묶음

    public Button plantButton; // 심기 버튼
    public Button harvestButton; // 수확 버튼
    public Button closebutton;
    public Button closebutton_2;

    private FarmGround currentGround;

    private void Start()
    {
        plantButton.onClick.AddListener(OnPlantClicked);
        harvestButton.onClick.AddListener(OnHarvestClicked);
        closebutton.onClick.AddListener(Hide);
    }

    public void Show(FarmGround ground)
    {
        currentGround = ground;

        // 밭의 상태에 따라 버튼 다르게 표시
        switch (ground.data.status)
        {
            case "empty":
                plantButton.gameObject.SetActive(true);
                harvestButton.gameObject.SetActive(false);
                break;
            case "grown":
                plantButton.gameObject.SetActive(false);
                harvestButton.gameObject.SetActive(true);
                break;
            default:
                root.SetActive(false);
                return;
        }

        // 화면 위치 이동
        Vector3 screenPos = Camera.main.WorldToScreenPoint(ground.transform.position + Vector3.up * 0.5f);
        root.transform.position = screenPos;

        root.SetActive(true);
    }

    public void Hide()
    {
        root.SetActive(false);

        currentGround = null;
    }

    private void OnPlantClicked()
    {
        UIManager.Instance.OpenPlantPopup(currentGround); // 팝업 띄우기
        Hide();
    }

    private void OnHarvestClicked()
    {
        currentGround.TryHarvest(); // 수확 실행
        Hide();
    }
}
