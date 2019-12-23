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
    public Text TimerComponent;
    public List<GameObject> Towers;
    private uint msDuration;
    private uint timePassed;

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
        if (EndGameSingleton.Instance.GameEnded) return;

        timePassed += timeLapsed;

        // Update timer
        if (TimerComponent)
        {
            var timeLeft = msDuration - timePassed;

            TimerComponent.text = TimeSpan
                .FromMilliseconds(timeLeft)
                .ToString(@"ss\:ff");
        }

        if (timePassed % msDuration == 0) 
        {
            // The game has ended
            var winnerTeam = GetWinnerTeam();
            EndGameSingleton.Instance.SetWinner(winnerTeam);
        }
    }

    int GetWinnerTeam() 
    {
        int team1LowestHp = -1;
        int team2LowestHp = -1;
        List<GameObject> team1Towers = new List<GameObject>();
        List<GameObject> team2Towers = new List<GameObject>();

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
                    team1Towers.Add(tower);

                    if (team1LowestHp == -1 || lifeComponent.lifeValue < team1LowestHp)
                        team1LowestHp = lifeComponent.lifeValue;

                    break;

                case TowerPosition.TopMiddle:
                case TowerPosition.TopLeft:
                case TowerPosition.TopRight:
                    team2Towers.Add(tower);

                    if (team2LowestHp == -1 || lifeComponent.lifeValue < team2LowestHp)
                        team2LowestHp = lifeComponent.lifeValue;

                    break;
            }
        }

        // If a team has more towers...
        if (team1Towers.Count > team2Towers.Count)
            return 1;
        else if (team2Towers.Count > team1Towers.Count)
            return 2;

        // Checking for the tower with the lowest HP...
        if (team1LowestHp > team2LowestHp)
            return 1;
        else if (team2LowestHp > team1LowestHp)
            return 2; 
        
        // Otherwise it's a draw...
        return 0;
    }
}
