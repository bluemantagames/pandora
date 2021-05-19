using UnityEngine;

namespace Pandora.Combat
{
    public interface DamageSource
    {
        GameObject GameObject { get; }
    }

    /// <summary>Damage incoming from base attack</summary>
    public class BaseAttack : DamageSource
    {
        public GameObject GameObject { get; private set; }

        public BaseAttack(GameObject gameObject)
        {
            this.GameObject = gameObject;
        }
    }

    public class GoldRewardEffect : DamageSource
    {
        public GameObject GameObject { get; private set; }

        public GoldRewardEffect(GameObject rewardObject)
        {
            GameObject = rewardObject;
        }
    }

    /// <summary>Damage incoming from tower base attack</summary>
    public class TowerBaseAttack : DamageSource
    {
        public GameObject GameObject { get; private set; }

        public TowerBaseAttack(GameObject gameObject)
        {
            this.GameObject = gameObject;
        }
    }


    /// <summary>Damage incoming from a spell</summary>
    public class SpellDamage : DamageSource
    {
        public GameObject GameObject { get; private set; }

        public SpellDamage(GameObject gameObject)
        {
            this.GameObject = gameObject;
        }
    }

    /// <summary>Damage incoming from a Debuff effect</summary>
    public class Debuff : DamageSource
    {
        public GameObject GameObject { get; private set; }
        public Effect Effect;

        public Debuff(GameObject gameObject, Effect effect)
        {
            this.GameObject = gameObject;
            this.Effect = effect;
        }
    }

    /// <summary>Damage incoming from a command</summary>
    public class UnitCommand : DamageSource
    {
        public GameObject GameObject { get; private set; }

        public UnitCommand(GameObject gameObject)
        {
            this.GameObject = gameObject;
        }
    }

    /// <summary>Damage incoming from the very same unit</summary>
    public class SelfDamage : DamageSource
    {
        public GameObject GameObject { get; private set; }

        public SelfDamage(GameObject gameObject)
        {
            this.GameObject = gameObject;
        }
    }
}