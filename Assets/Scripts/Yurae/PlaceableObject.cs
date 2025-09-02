// Unity
using UnityEngine;
using static TMPro.Examples.TMP_ExampleScript_01;

public enum PlaceType
{
    Tile, Object, Plant
}

[DisallowMultipleComponent]
public class PlaceableObject : MonoBehaviour
{
    [Header("Postion (Grid)")]
    [SerializeField] private Vector2Int gridPosition;

    [Header("ID")]
    [SerializeField] private string id;
    [SerializeField] private long numberId;

    [Header("Rotation Sprite")]
    [SerializeField] private Sprite[] rotationSprites;
    private int spriteIndex; // => Modify Rotation Enum

    private SpriteRenderer spriteRenderer;
    private Garden.RotationEnum currentRotation;

    public string GetID()
    {
        return id;
    }

    public long GetNoID()
    {
        return numberId;
    }

    private void OnEnable()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = GridManager.Instance.GetGridPosition(mousePos);

        transform.position = GridManager.Instance.GetWorldPosition(gridPos.x, gridPos.y);

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetPosition(Vector2Int gridPos)
    {
        gridPosition = gridPos;
        transform.position = GridManager.Instance.GetWorldPosition(gridPos.x, gridPos.y);
    }

    public Vector2Int GetPosition()
    {
        return gridPosition;
    }

    public void SetRotation(Garden.RotationEnum rotation)
    {
        currentRotation = rotation;

        Rotation(false);
    }

    /// <summary>
    /// Rotation Method 
    /// </summary>
    public void Rotation(bool isNeedSave = true)
    {
        // ���� ���� ��ü
        if (rotationSprites.Length == 0)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            if (spriteRenderer.flipX == false) currentRotation = Garden.RotationEnum.R0;
            else currentRotation = Garden.RotationEnum.R180;
        }
        // 4���� ȸ�� ��ü
        else
        {
            if (currentRotation == Garden.RotationEnum.R0)
            {
                currentRotation = Garden.RotationEnum.R90;
                spriteRenderer.sprite = rotationSprites[0];
                spriteRenderer.flipX = true;
            }
            else if (currentRotation == Garden.RotationEnum.R90)
            {
                currentRotation = Garden.RotationEnum.R180;
                spriteRenderer.sprite = rotationSprites[1];
                spriteRenderer.flipX = false;
            }
            else if (currentRotation == Garden.RotationEnum.R180)
            {
                currentRotation = Garden.RotationEnum.R270;
                spriteRenderer.sprite = rotationSprites[1];
                spriteRenderer.flipX = true;
            }
            else if (currentRotation == Garden.RotationEnum.R270) {
                currentRotation = Garden.RotationEnum.R0;
                spriteRenderer.sprite = rotationSprites[0];
                spriteRenderer.flipX = false;
            }
        }

        // �������� ������ �ʿ��� ���
        if (isNeedSave)
        {
            // Update Rotation To Server
            GardenControllerAPI.RotateGardenObject(
              gridPosition.x, gridPosition.y, numberId, currentRotation,
              onSuccess: (result) =>
              {
                  Debug.Log("ȸ�� ����! ���� ����: " + result);
              },
              onError: (error) =>
              {
                  Debug.LogError("ȸ�� ����: " + error);
              }
          );
        }
    }
}
