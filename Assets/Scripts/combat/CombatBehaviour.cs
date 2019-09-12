using Pandora;
using UnityEngine;

namespace Pandora.Combat {
    interface CombatBehaviour {
        /// <summary>True if the unit is currently in combat</summary>
        bool isAttacking { get; }
        /// <summary>Combat type of this behaviour</summary>
        CombatType combatType { get; }

        /// <summary>Begins attacking an enemy</summary>
        void AttackEnemy(Enemy target, uint timeLapse);

        /// <summary>Stops attacking an enemy</summary>
        void StopAttacking();

        /// <summary>Called if a launched projectile collided</summary>
        void ProjectileCollided();

        /// <summary>Called on unit death</summary>
        void OnDead();

        /// <summary>Returns whether the unit should aggro another unity</summary>
        bool IsInAggroRange(Enemy enemy);

        /// <summary>Returns whether the unit is in attack range</summary>
        bool IsInAttackRange(Enemy enemy);
    }
}