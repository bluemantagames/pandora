using Pandora.Network;
using Pandora.Network.Messages;

class EndGameSingleton {
    static EndGameSingleton _instance = null;
    public bool GameEnded { get; private set; }
    public int WinnerTeam { get; private set; }

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

    public void SetWinner(int winnerTeam)
    {
        GameEnded = true;
        WinnerTeam = winnerTeam;

        var matchFinishedMessage = new MatchFinishedMessage 
        { 
            WinnerTeam = winnerTeam 
        };

        NetworkControllerSingleton.instance.EnqueueMessage(matchFinishedMessage);
    }
}