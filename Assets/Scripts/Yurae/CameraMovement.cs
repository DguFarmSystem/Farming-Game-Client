// Unity
using UnityEngine;

[DisallowMultipleComponent]
public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [Header("카메라 바운더리 관련")]
    [SerializeField] private float xBoundary = 5f;
    [SerializeField] private float yBoundary = 10f;

    [Header("카메라 줌 관련")]
    [SerializeField] private float zoomInSize;
    [SerializeField] private float zoomOutSize;


    private void Update()
    {
        HandleMovement();
        HandleZoomInOut();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(moveX, moveY, 0f).normalized;

        Vector3 moveDelta = direction * moveSpeed * Time.deltaTime;

        transform.position = new Vector3(
            transform.position.x + moveDelta.x,
            transform.position.y + moveDelta.y,
            transform.position.z
        );

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, -xBoundary, xBoundary),
            Mathf.Clamp(transform.position.y, 0, yBoundary),
            transform.position.z
        );
    }

    private void HandleZoomInOut()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            Camera.main.orthographicSize -= scroll;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, zoomInSize, zoomOutSize);
        }
    }
}
