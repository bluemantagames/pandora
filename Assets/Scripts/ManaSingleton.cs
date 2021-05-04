using System;
using Pandora.Resource;
using Pandora;
using Pandora.Resource.Mana;

public class ManaSingleton
{
    static ManaSingleton _instance = null;
    ResourceWallet<ManaEvent> manaWallet;
    ResourceWallet<ManaEvent> enemyManaWallet;

    public int ManaValue
    {
        get
        {
            return manaWallet.Resource;
        }
    }

    public int MaxMana
    {
        get
        {
            return manaWallet.ResourceUpperCap.Value;
        }
    }

    public int MinMana
    {
        get
        {
            return manaWallet.ResourceLowerCap.Value;
        }
    }

    public Decimal ManaPerTick = 1m;

    // This is only used in dev
    public int ManaUnit { get; set; } = 0;

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
        var walletsComponent = MapComponent.Instance.GetComponent<WalletsComponent>();

        manaWallet = walletsComponent.ManaWallet;
        enemyManaWallet = walletsComponent.EnemyManaWallet;
    }

    public void UpdateMana(Decimal newValue, ResourceWallet<ManaEvent> manaWallet)
    {
        var difference = Decimal.ToInt32(Decimal.Floor(newValue - manaWallet.Resource));

        if (difference < 0)
            manaWallet.SpendResource(-difference);
        else
            manaWallet.AddResource(difference);
    }

    public void UpdateMana(Decimal newValue)
    {
        UpdateMana(newValue, manaWallet);
    }

    public void UpdateEnemyMana(Decimal newValue)
    {
        UpdateMana(newValue, enemyManaWallet);
    }
}