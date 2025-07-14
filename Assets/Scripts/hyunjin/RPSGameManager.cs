using Fusion;
using UnityEngine;

public class RPSGameManager : NetworkBehaviour
{
    [Networked] private TickTimer roundTimer { get; set; }
    private NetworkPlayer[] players;

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        players = FindObjectsOfType<NetworkPlayer>();

        if (players == null || players.Length < 2)
        {
            Debug.Log("2명 접속 전. 대기 중...");
            return;
        }

        if (roundTimer.Expired(Runner))
        {
            JudgeRound();
        }
    }

    void StartSingleRound()
    {
        roundTimer = TickTimer.CreateFromSeconds(Runner, 3f);
        foreach (var p in players)
            p.CurrentChoice = RPSChoice.NONE;

        Debug.Log("🎮 3초 안에 선택하세요!");
    }

    void JudgeRound()
    {
        if (players == null || players.Length < 2)
        {
            Debug.LogWarning("JudgeRound() 호출됐지만 플레이어 수 부족!");
            return;
        }

        var p1 = players[0];
        var p2 = players[1];

        var c1 = p1.CurrentChoice;
        var c2 = p2.CurrentChoice;

        int result = Judge(c1, c2);

        string resultText = result switch
        {
            0 => "무승부!",
            1 => "플레이어 1 승리!",
            2 => "플레이어 2 승리!",
            _ => "알 수 없는 결과"
        };

        Debug.Log($"판정: P1({c1}) vs P2({c2}) → {resultText}");

        // 라운드 종료 후 다음 흐름 추가 가능
    }

    int Judge(RPSChoice a, RPSChoice b)
    {
        if (a == RPSChoice.NONE && b == RPSChoice.NONE) return 0;
        if (a == RPSChoice.NONE) return 2;
        if (b == RPSChoice.NONE) return 1;
        if (a == b) return 0;

        if ((a == RPSChoice.ROCK && b == RPSChoice.SCISSORS) ||
            (a == RPSChoice.PAPER && b == RPSChoice.ROCK) ||
            (a == RPSChoice.SCISSORS && b == RPSChoice.PAPER))
            return 1;

        return 2;
    }
}
