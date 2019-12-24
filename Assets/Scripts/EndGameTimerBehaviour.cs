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

    void Start()
    {
        timePassed = 0;
        msDuration = GameDurationSeconds * 1000;

        var engineComponent = GetComponent<EngineComponent>();

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

        if (timePassed % msDuration <= 0) 
        {
            // The game has ended
            var winnerTeam = GetWinnerTeam();
            EndGameSingleton.Instance.SetWinner(winnerTeam);
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
        int winnerSide = 0;
        List<GameObject> bottomTowers = new List<GameObject>();
        List<GameObject> topTowers = new List<GameObject>();

        foreach (var tower in Towers)
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
            winnerSide = 1;
        else if (topTowers.Count > bottomTowers.Count)
            winnerSide = 2;

        // Checking for the tower with the lowest HP...
        if (bottomLowestHp > topLowestHp)
            winnerSide = 1;
        else if (topLowestHp > bottomLowestHp)
            winnerSide = 2; 
        
        // Checking which team actually won
        if (winnerSide == 0 || TeamComponent.assignedTeam == 1) 
            return winnerSide;
        else
            return winnerSide == 1 ? 2 : 1;
    }
}
