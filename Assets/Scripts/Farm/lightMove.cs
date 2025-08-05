using UnityEngine;

public class lightMove : MonoBehaviour
{
   public float rotateSpeed = 45f; // 초당 90도 회전

    void Update()
    {
        transform.Rotate(0f, 0f, -rotateSpeed * Time.deltaTime); // 시계방향은 Z축 음수 방향
    }
}
