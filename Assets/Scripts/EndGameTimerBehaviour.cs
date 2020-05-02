using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pandora.Engine;
using Pandora;

public class EndGameTimerBehaviour : MonoBehaviour, EngineBehaviour
{
    public uint GameDurationSeconds = 10;
    public string ComponentName {
        get => "EndGameBehaviour";
    }
    public Text TitleComponent;
    public Text TimerComponent;
    public List<GameObject> Towers;
    private uint msDuration;
    private uint timePassed;
    private bool winnerTextSet = false;

    EngineComponent engineComponent;

    void Start()
    {
        timePassed = 0;
        msDuration = GameDurationSeconds * 1000;

        engineComponent = GetComponent<EngineComponent>();

        if (engineComponent != null) 
        {
            engineComponent.Engine.AddBehaviour(this);
        }
    }

    public void TickUpdate(uint timeLapsed)
    {                
        if (EndGameSingleton.Instance.GameEnded) 
        {
            if (!winnerTextSet)
            {
                SetWinnerText(EndGameSingleton.Instance.WinnerTeam);
                winnerTextSet = true;
            }

            return;
        };

        timePassed += timeLapsed;

        if (TimerComponent)
        {
            var timeLeft = msDuration - timePassed;

            TimerComponent.text = TimeSpan
                .FromMilliseconds(timeLeft)
                .ToString(@"mm\:ss\:ff");
        }

        if (timePassed % msDuration <= 0 && engineComponent != null) 
        {
            // The game has ended
            var winnerTeam = GetWinnerTeam();

            EndGameSingleton.Instance.SetWinner(
                winnerTeam, 
                engineComponent.Engine.totalElapsed
            );
        }
    }
    
    public void SetWinnerText(int winnerTeam)
    {
        if (TimerComponent == null) return;

        if (TitleComponent != null)
        {
            TitleComponent.text = "Match result:";
        }

        if (winnerTeam == 0)
        {
            TimerComponent.text = $"DRAW";
        }
        else 
        {
            TimerComponent.text = $"TEAM {winnerTeam} WON";
        }
    }

    int GetWinnerTeam() 
    {
        int bottomLowestHp = -1;
        int topLowestHp = -1;
        List<GameObject> bottomTowers = new List<GameObject>();
        List<GameObject> topTowers = new List<GameObject>();

        foreach (var tower in Towers)
        {
            var towerPositionComponent = tower.GetComponent<TowerPositionComponent>();
            var lifeComponent = tower.GetComponent<LifeComponent>();

            if (towerPositionComponent == null || lifeComponent == null) continue;

            if (lifeComponent.lifeValue <= 0) continue;

            switch (towerPositionComponent.EngineTowerPosition)
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
