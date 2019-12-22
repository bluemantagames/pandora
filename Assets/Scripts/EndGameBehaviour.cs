using System;
using UnityEngine;
using UnityEngine.UI;
using Pandora.Engine;

public class EndGameBehaviour : MonoBehaviour, EngineBehaviour
{
    public uint GameDurationSeconds = 10;
    public string ComponentName {
        get => "EndGameBehaviour";
    }
    public Text TimerComponent;
    private uint msDuration;
    private uint timePassed;
    private bool gameEnded = false;

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
        if (gameEnded) return;

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
            Debug.Log("[TIMER] GAME OVER!");
            gameEnded = true;
        }
    }
}
