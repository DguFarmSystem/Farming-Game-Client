using Fusion;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [Networked] public RPSChoice CurrentChoice { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            if (CurrentChoice == RPSChoice.NONE)
                CurrentChoice = input.Choice;
        }
    }
}
