using UnityEngine;

namespace Pandora.Combat {
    public class DamageSource {}

    /// <summary>Damage incoming from base attack</summary>
    public class BaseAttack: DamageSource {
        public GameObject GameObject;
    }

    /// <summary>Damage incoming from a Debuff effect</summary>
    public class Debuff: DamageSource {
        public GameObject Source;
        public Effect Effect;
    }

    /// <summary>Damage incoming from a command</summary>
    public class UnitCommand: DamageSource {
        public GameObject Source;
    }
}