using Pandora;
using UnityEngine;

namespace Pandora.Combat
{
    public interface CombatVFXFixer
    {
        /// <summary>Fix the VFX based on the enemy direction.</summary>
        void FixVFX(Vector2 source, Vector2 target);
    }
}