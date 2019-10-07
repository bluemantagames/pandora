using UnityEngine;

namespace Pandora.Combat {
    public interface CombatEffect {
        void Apply(GameObject origin, GameObject target);
    }
}