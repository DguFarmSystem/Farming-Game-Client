// Fustion
using Fusion;

// Unity
using UnityEngine;

[DisallowMultipleComponent]
public class Player : NetworkBehaviour
{
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            InitCamera();
        }
    }

    private void InitCamera()
    {
        CameraController cameraController = Camera.main.GetComponent<CameraController>();

        if (cameraController != null) cameraController.SetTarget(transform);
    }
}
