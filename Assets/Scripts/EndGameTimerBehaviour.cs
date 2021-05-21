using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Pandora.Engine;
using Pandora;

public class EndGameTimerBehaviour : MonoBehaviour, EngineBehaviour
{
    public uint GameDurationSeconds = 180;
    public string ComponentName
    {
        get => "EndGameBehaviour";
    }
    public Text TitleComponent;
    public Text TimerComponent;
    private uint msDuration;
    private uint timePassed;
    private bool winnerTextSet = false;
    Text text;
    PandoraEngine engine;

    EngineComponent engineComponent;

    void Start()
    {
        timePassed = 0;
        msDuration = GameDurationSeconds * 1000;

        text = GetComponent<Text>();

        engine = MapComponent.Instance.engine;

        engine.AddBehaviour(this);
    }

    public void TickUpdate(uint timeLapsed)
    {
        if (EndGameSingleton.Instance.GameEnded)
        {
            if (!winnerTextSet)
            {
                // TODO: Match ended
                winnerTextSet = true;
            }

            return;
        };

        timePassed += timeLapsed;

        var timeLeft = msDuration - timePassed;

        text.text = TimeSpan
            .FromMilliseconds(timeLeft)
            .ToString(@"mm\:ss");

        if (timePassed % msDuration <= 0 && engine != null)
        {
            // The game has ended
            var winnerTeam = GetWinnerTeam();

            EndGameSingleton.Instance.SetWinner(
                winnerTeam,
                engine.TotalElapsed
            );
        }
    }

    int GetWinnerTeam()
    {
        int bottomLowestHp = -1;
        int topLowestHp = -1;
        List<GameObject> bottomTowers = new List<GameObject>();
        List<GameObject> topTowers = new List<GameObject>();

        var towers = 
            from entity in MapComponent.Instance.engine.Entities
            where entity.GameObject.GetComponent<TowerPositionComponent>() != null
            select entity.GameObject;

        foreach (var tower in towers)
        {
            var towerPositionComponent = tower.GetComponent<TowerPositionComponent>();
            var lifeComponent = tower.GetComponent<LifeComponent>();

            if (towerPositionComponent == null || lifeComponent == null) continue;

            if (lifeComponent.lifeValue <= 0) continue;

            switch (towerPositionComponent.WorldTowerPosition)
            {
                case TowerPosition.BottomMiddle:
                case TowerPosition.BottomLeft:
                case TowerPosition.BottomRight:
                    bottomTowers.Add(tower);

                    if (bottomLowestHp == -1 || lifeComponent.lifeValue < bottomLowestHp)
                        bottomLowestHp = lifeComponent.lifeValue;

                    break;

                case TowerPosition.TopMiddle:
                case TowerPosition.TopLeft:
                case TowerPosition.TopRight:
                    topTowers.Add(tower);

                    if (topLowestHp == -1 || lifeComponent.lifeValue < topLowestHp)
                        topLowestHp = lifeComponent.lifeValue;

                    break;
            }
        }

        // If a team has more towers...
        if (bottomTowers.Count > topTowers.Count)
            return TeamComponent.bottomTeam;
        else if (topTowers.Count > bottomTowers.Count)
            return TeamComponent.topTeam;

        // Checking for the tower with the lowest HP...
        if (bottomLowestHp > topLowestHp)
            return TeamComponent.bottomTeam;
        else if (topLowestHp > bottomLowestHp)
            return TeamComponent.topTeam;

        // Otherwise it's a draw...
        return 0;
    }
}
