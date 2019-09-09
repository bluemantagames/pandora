using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mana : MonoBehaviour
{
    float manaValue;
    public float maxMana = 10;
    public float minMana = 0;
    public float initialMana = 5;

    // Start is called before the first frame update
    void Start()
    {
        manaValue = initialMana;
    }

    public void updateMana(float newValue)
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

    public float getMana()
    {
        return manaValue;
    }
}