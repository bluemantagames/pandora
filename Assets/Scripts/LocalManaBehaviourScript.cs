using System;
using UnityEngine;
using Pandora.Network;

public class LocalManaBehaviourScript : MonoBehaviour
{
    public bool Enabled = true;
    public int ManaEveryTimelapse = 10;
    public float StepAmount = 0.2f;
    static public float RoundingTimelapse = 2.8f;

    float manaPerStep;
    float timer;
    float roundingTimer;

    private void Start()
    {
        manaPerStep = StepAmount * ManaEveryTimelapse / RoundingTimelapse;
        timer = StepAmount;
        roundingTimer = RoundingTimelapse;
    }

    private void Update()
    {
        if (NetworkControllerSingleton.instance.matchStarted)
        {
            Destroy(this);

            return;
        }

        roundingTimer -= Time.deltaTime;
        timer -= Time.deltaTime;

        if (Mathf.Approximately(ManaSingleton.Instance.ManaValue, ManaSingleton.Instance.MaxMana))
        {
            return;
        }

        bool isTimerEnd = timer < 0 || Mathf.Approximately(timer, 0f);
        bool isRoundingTimerEnd = roundingTimer < 0 || Mathf.Approximately(roundingTimer, 0f);

        if (isTimerEnd)
        {
            timer = StepAmount;

            if (isRoundingTimerEnd)
            {
                ManaSingleton.Instance.ManaUnit += ManaEveryTimelapse;
                roundingTimer = RoundingTimelapse;

                UpdateMana(ManaSingleton.Instance.ManaUnit);
                UpdateEnemyMana(ManaSingleton.Instance.ManaUnit);
            }
            else
            {
                UpdateMana(ManaSingleton.Instance.ManaValue + manaPerStep);
                UpdateEnemyMana(ManaSingleton.Instance.ManaValue + manaPerStep);
            }
        }
    }

    void UpdateMana(float value)
    {
        if (!Enabled)
        {
            return;
        }

        ManaSingleton.Instance.UpdateMana(new Decimal(value));
    }

    void UpdateEnemyMana(float value)
    {
        if (!Enabled)
        {
            return;
        }

        ManaSingleton.Instance.UpdateEnemyMana(new Decimal(value));
    }
}
