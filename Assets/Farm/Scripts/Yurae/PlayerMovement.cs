using Fusion;
using UnityEngine;

public struct PlayerInputData : INetworkInput
{
    public float horizontal;
    public float vertical;
}

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;

    [Networked] private Vector3 Velocity { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out PlayerInputData input))
        {
            // 서버만 위치 계산을 함
            if (Object.HasStateAuthority)
            {
                Vector3 moveDir = new Vector3(input.horizontal, 0, input.vertical).normalized;
                Velocity = moveDir * moveSpeed;
                transform.position += Velocity * Runner.DeltaTime;
            }
        }
    }
}
