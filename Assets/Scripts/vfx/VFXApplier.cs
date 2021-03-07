using UnityEngine;

namespace Pandora.VFX
{
    interface VFXApplier
    {
        /// <summary>Applies the VFX to the target gameobject</summary>
        GameObject Apply(GameObject target);
    }
}