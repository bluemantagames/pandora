using CRclone;
using UnityEngine;

namespace CRclone.Spell {
    public interface ProjectileSpell {
        MapListener map { get; set; }

        void SpellCollided(Vector2 cell);
    }
}