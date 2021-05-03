using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Resource;
using Pandora;
using Pandora.Resource.Mana;

public class ManaSingleton
{
    public static float manaValue { get; private set; } = 0;
    public static float maxMana = 100f;
    public static float minMana = 0f;

    // This is only used in dev
    public static float manaUnit { get; set; } = 0;

    public static void UpdateMana(float newValue, ResourceWallet<ManaEvent> manaWallet)
    {
        var difference = Mathf.FloorToInt(newValue - manaWallet.Resource);

        if (difference < 0)
        {
            manaWallet.SpendResource(-difference);
        }
        else
        {
            manaWallet.AddResource(difference);
        }
    }

    public static void UpdateMana(float newValue)
    {
        var manaWallet = MapComponent.Instance.GetComponent<WalletsComponent>().ManaWallet;
        UpdateMana(newValue, manaWallet);
    }

    public static void UpdateEnemyMana(float newValue)
    {
        var manaWallet = MapComponent.Instance.GetComponent<WalletsComponent>().EnemyManaWallet;
        UpdateMana(newValue, manaWallet);
    }
}