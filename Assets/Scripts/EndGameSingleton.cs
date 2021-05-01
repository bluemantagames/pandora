using Pandora.Network;
using Pandora.Network.Messages;
using UnityEngine;
using Pandora;
using Pandora.UI.HUD;

class EndGameSingleton
{
    static EndGameSingleton _instance = null;
    public bool GameEnded { get; private set; } = false;
    public int WinnerTeam { get; private set; } = 0;

    static public EndGameSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new EndGameSingleton();
            }

            return _instance;
        }
    }

    public static void Reset()
    {
        _instance = null;
    }

    public void SetWinner(int winnerTeam, ulong elapsedMs)
    {
        if (GameEnded) return;

        GameEnded = true;
        WinnerTeam = winnerTeam;

        Logger.Debug($"[ENDGAME] TEAM {WinnerTeam} WON!");

        var matchFinishedMessage = new MatchFinishedMessage
        {
            WinnerTeam = winnerTeam,
            ElapsedMs = elapsedMs
        };

        NetworkControllerSingleton.instance.EnqueueMessage(matchFinishedMessage);

        // Send endgame analytics
        var token = PlayerModelSingleton.instance.Token;
        var matchToken = NetworkControllerSingleton.instance.CurrentMatchToken;

        if (token != null && matchToken != null)
        {
            Logger.Debug("Sending towers damages analytics to the server...");
            var towersDamages = JsonUtility.ToJson(MatchInfoSingleton.Instance.GetSerializableTowersDamages());
            _ = ApiControllerSingleton.instance.SendAnalytics(matchToken, towersDamages, token);

            Logger.Debug("Sending mulligan analytics to the server...");
            var mulliganCards = JsonUtility.ToJson(MatchInfoSingleton.Instance.GetSerializableMulliganCards());
            _ = ApiControllerSingleton.instance.SendAnalytics(matchToken, mulliganCards, token);
        }

        var container = GameObject.Find("MatchEndPanelContainer");

        container.GetComponent<MatchEndPanelAnimation>().StartAnimation(winnerTeam);
    }
}