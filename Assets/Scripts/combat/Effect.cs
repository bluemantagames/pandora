using UnityEngine;

namespace Pandora.Combat
{
    public interface Effect
    {
        bool IsDisabled { get; set; }

        Effect Apply(GameObject origin, GameObject target);

        void Unapply(GameObject target);
    }
}