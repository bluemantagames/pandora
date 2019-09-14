using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaSingleton
{
    public static float manaValue { get; private set; }
    public static float maxMana = 100f;
    public static float minMana = 0f;

    public static void updateMana(float newValue)
    {
        if (newValue < minMana)
        {
            manaValue = minMana;
        }

        if (newValue > maxMana)
        {
            manaValue = maxMana;
        }

        manaValue = newValue;
    }
}