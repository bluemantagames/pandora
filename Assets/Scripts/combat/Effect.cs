using UnityEngine;

namespace Pandora.Combat {
    public interface Effect {
        Effect Apply(GameObject origin, GameObject target);

        void Unapply(GameObject target);
    }
}