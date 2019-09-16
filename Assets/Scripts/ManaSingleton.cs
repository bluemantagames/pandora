using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaSingleton
{
    public static float manaValue { get; private set; } = 0;
    public static float maxMana = 100f;
    public static float minMana = 0f;

    // This is only used in dev
    public static float manaUnit { get; set; } = 0;

    public static void UpdateMana(float newValue)
    {
        if (newValue <= minMana)
        {
            manaValue = minMana;
            return;
        }

        if (newValue >= maxMana)
        {
            manaValue = maxMana;
            return;
        }

        manaValue = newValue;
    }
}