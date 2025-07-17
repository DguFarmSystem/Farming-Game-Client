using Fusion;
using UnityEngine;

public enum RPSResult { NONE, WIN, LOSE, DRAW }

public class RPSPlayer : NetworkBehaviour
{
    [Networked] public RPSChoice CurrentChoice { get; set; }
    [Networked] public RPSChoice OpponentChoice { get; set; }
    [Networked] public RPSResult Result { get; set; }
    [Networked] public float RemainingTime { get; set; }

    private RPSResult _lastResult = RPSResult.NONE;

    public override void FixedUpdateNetwork()
    {
        // 입력처리 (서버)
        if (GetInput(out NetworkInputData input)) {
            if (CurrentChoice == RPSChoice.NONE)
                CurrentChoice = input.Choice;
        }

        // 결과 출력(클라이언트)
        #if !UNITY_SERVER
            var ui = FindObjectOfType<RPSUIManager>();
            if (!ui) return;
            ui.PrintRemainingTime(RemainingTime);
            if (Object.HasInputAuthority && Result != RPSResult.NONE && Result != _lastResult) {
                ShowResultUI();
                _lastResult = Result;
            }
        #endif
    }

    #if !UNITY_SERVER
        public void ShowResultUI()
        {
            var ui = FindObjectOfType<RPSUIManager>();
            if (!ui) return;

            string resultText = Result switch
            {
                RPSResult.WIN => "win",
                RPSResult.LOSE => "lose",
                RPSResult.DRAW => "draw",
                _ => ""
            };

            ui.ShowResult(CurrentChoice, OpponentChoice, resultText);
        }
    #endif
}
