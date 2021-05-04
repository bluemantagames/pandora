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
        var manaValue = Mathf.FloorToInt(ManaSingleton.Instance.manaValue);
        var maxMana = ManaSingleton.Instance.maxMana;

        GetComponent<Text>().text = $"Mana: {manaValue} / {maxMana}";
    }
}
