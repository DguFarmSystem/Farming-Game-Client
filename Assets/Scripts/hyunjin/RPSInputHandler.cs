using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;

public class RPSInputHandler : MonoBehaviour
{
    private RPSChoice pendingChoice = RPSChoice.NONE;

    public void SetChoice(RPSChoice choice) {
        pendingChoice = choice;
    }

    public RPSChoice ConsumeInput() {
        RPSChoice temp = pendingChoice;
        pendingChoice = RPSChoice.NONE; // 입력 초기화
        return temp;
    }
}