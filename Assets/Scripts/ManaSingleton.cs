using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaSingleton
{
    public static int manaValue { get; private set; }
    public static int maxMana = 100;
    public static int minMana = 0;

    public static void updateMana(int newValue)
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