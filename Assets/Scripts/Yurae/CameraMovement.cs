// Unity
using UnityEngine;

[DisallowMultipleComponent]
public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float boundary = 5f;

    private void Update()
    {
        HandleMovement();
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
            Mathf.Clamp(transform.position.x, -boundary, boundary),
            Mathf.Clamp(transform.position.y, -boundary, boundary),
            transform.position.z
        );
    }
}
