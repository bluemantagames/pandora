using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Resource;
using Pandora;
using Pandora.Resource.Mana;

public class ManaSingleton
{
    static ManaSingleton _instance = null;
    ResourceWallet<ManaEvent> manaWallet;
    ResourceWallet<ManaEvent> enemyManaWallet;

    public float manaValue
    {
        get
        {
            return manaWallet.Resource;
        }
    }

    public float maxMana
    {
        get
        {
            return manaWallet.ResourceUpperCap.Value;
        }
    }

    public float minMana
    {
        get
        {
            return manaWallet.ResourceLowerCap.Value;
        }
    }

    // This is only used in dev
    public float manaUnit { get; set; } = 0;

    static public ManaSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ManaSingleton();
            }

            return _instance;
        }
    }

    public ManaSingleton()
    {
        manaWallet = MapComponent.Instance.GetComponent<WalletsComponent>().ManaWallet;
        enemyManaWallet = MapComponent.Instance.GetComponent<WalletsComponent>().EnemyManaWallet;
    }


    public void UpdateMana(float newValue, ResourceWallet<ManaEvent> manaWallet)
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

    public void UpdateMana(float newValue)
    {
        var manaWallet = MapComponent.Instance.GetComponent<WalletsComponent>().ManaWallet;
        UpdateMana(newValue, manaWallet);
    }

    public void UpdateEnemyMana(float newValue)
    {
        var manaWallet = MapComponent.Instance.GetComponent<WalletsComponent>().EnemyManaWallet;
        UpdateMana(newValue, manaWallet);
    }
}