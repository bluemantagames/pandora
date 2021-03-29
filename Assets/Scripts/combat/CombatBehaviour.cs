using Pandora;
using UnityEngine;

namespace Pandora.Combat
{
    public interface CombatBehaviour
    {
        /// <summary>True if the unit is currently in combat</summary>
        bool isAttacking { get; }

        /// <summary>True if the combat is disabled for this unit</summary>
        bool IsDisabled { get; set; }

        /// <summary>Combat type of this behaviour</summary>
        CombatType combatType { get; }

        /// <summary>Begins attacking an enemy</summary>
        void AttackEnemy(Enemy target, uint timeLapse);

        /// <summary>Stops attacking an enemy</summary>
        void StopAttacking();

        /// <summary>Called if a launched projectile collided</summary>
        void ProjectileCollided(Enemy target);

        /// <summary>Called on unit death</summary>
        void OnDead();

        /// <summary>Returns whether the unit should aggro another unity</summary>
        bool IsInAggroRange(Enemy enemy);

        /// <summary>Returns whether the unit aggros another unit in a cell</summary>
        bool IsInAggroRange(GridCell cell);

        /// <summary>Returns whether the unit is in attack range</summary>
        bool IsInAttackRange(Enemy enemy);
    }
}