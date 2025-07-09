// Fusion
using Fusion;

// Unity
using UnityEngine;

public struct PlayerInputData : INetworkInput
{
    public float horizontal;
    public float vertical;
}

[DisallowMultipleComponent]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rigid;

    [Networked] private Vector3 Velocity { get; set; }

    public override void Spawned()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out PlayerInputData input))
        {
            if (Object.HasStateAuthority)
            {
                Vector2 moveDir = new Vector2(input.horizontal, input.vertical).normalized;
                Velocity = moveDir * moveSpeed;

                // 충돌 감지 포함된 이동 처리
                rigid.MovePosition(new Vector3(rigid.position.x, rigid.position.y, transform.position.z) + Velocity * Runner.DeltaTime);
            }
        }
    }
}
