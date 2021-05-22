using Pandora;
using UnityEngine;

namespace Pandora.Combat
{
    public interface CombatVFXFixer
    {
        /// <summary>Fix the VFX rotation based on the enemy direction.</summary>
        Quaternion FixedShotRotation(Vector2Int enemyDirection);
    }
}