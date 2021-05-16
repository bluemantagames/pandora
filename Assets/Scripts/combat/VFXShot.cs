using Pandora;
using UnityEngine;

namespace Pandora.Combat
{
    public interface VFXShot
    {
        /// <summary>The enemy direction</summary>
        Vector2 EnemyDirection { get; set; }
    }
}