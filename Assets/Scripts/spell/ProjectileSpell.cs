using Pandora;
using UnityEngine;

namespace Pandora.Spell {
    public interface ProjectileSpell {
        MapComponent map { get; set; }

        void SpellCollided(GridCell cell);
    }
}