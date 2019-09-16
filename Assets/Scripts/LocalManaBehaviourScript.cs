using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Network;

public class LocalManaBehaviourScript : MonoBehaviour
{
    public bool enabled = true;
    public int manaEveryTimelapse = 10;
    public float stepAmount = 0.2f;
    public float roundingTimelapse = 2.8f;

    float manaPerStep;
    float timer;
    float roundingTimer;

    private void Start()
    {
        manaPerStep = stepAmount * manaEveryTimelapse / roundingTimelapse;
        timer = stepAmount;
        roundingTimer = roundingTimelapse;
    }

    private void Update()
    {
        if (NetworkControllerSingleton.instance.matchStarted)
        {
            return;
        }

        roundingTimer -= Time.fixedDeltaTime;
        timer -= Time.fixedDeltaTime;

        if (Mathf.Approximately(ManaSingleton.manaValue, ManaSingleton.maxMana))
        {
            return;
        }

        bool isTimerEnd = timer < 0 || Mathf.Approximately(timer, 0f);
        bool isRoundingTimerEnd = roundingTimer < 0 || Mathf.Approximately(roundingTimer, 0f);

        if (isTimerEnd)
        {
            timer = stepAmount;

            if (isRoundingTimerEnd)
            {
                ManaSingleton.manaUnit += manaEveryTimelapse;
                roundingTimer = roundingTimelapse;

                UpdateMana(ManaSingleton.manaUnit);
            }
            else
            {
                UpdateMana(ManaSingleton.manaValue + manaPerStep);
            }
        }
    }

    void UpdateMana(float value)
    {
        if (!enabled)
        {
            return;
        }

        ManaSingleton.UpdateMana(value);
    }
}
