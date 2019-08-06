using CRclone;
using UnityEngine;

namespace CRclone.Spell {
    public interface ProjectileSpell {
        MapComponent map { get; set; }

        void SpellCollided(GridCell cell);
    }
}