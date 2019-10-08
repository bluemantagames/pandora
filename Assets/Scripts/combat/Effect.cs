using UnityEngine;

namespace Pandora.Combat
{
    public interface Effect
    {

        /// <summary>
        /// Applies the effect on the target. This is expected to actually
        /// add a component
        /// </summary>
        Effect Apply(GameObject origin, GameObject target);

        /// <summary>
        /// Removes the effect from the target
        /// </summary>
        void Unapply(GameObject target);
    }
}