using UnityEngine;
using Fusion;

public enum RPSChoice : byte {
    NONE,
    ROCK,
    PAPER,
    SCISSORS
}

public struct NetworkInputData : INetworkInput
{
    public RPSChoice Choice;
}