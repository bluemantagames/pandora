using System.Collections.Generic;
using UnityEngine;

namespace Pandora.Combat
{
    public interface AreaDamage
    {
        /// <summary>Calculate the area damage for a specific type of attack.</summary>
        Dictionary<GameObject, int> CalculateAreaDamages(Enemy target);
    }
}