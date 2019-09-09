using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaTextBehaviour : MonoBehaviour
{
    void Awake()
    {
        UpdateText();
    }

    void Update()
    {
        UpdateText();
    }

    void UpdateText()
    {
        var manaValue = ManaSingleton.manaValue;
        var maxMana = ManaSingleton.maxMana;

        GetComponent<Text>().text = $"Mana: {manaValue} / {maxMana}";
    }
}
