using Pandora;
using UnityEngine;

namespace Pandora.Combat {
    interface CombatBehaviour {
        /** True if the unit is currently in combat */
        bool isAttacking { get; }
        CombatType combatType { get; }

        /** Begins attacking an enemy */
        void AttackEnemy(Enemy target);
        /** Stops attacking an enemy */
        void StopAttacking();
        /** Checks whether a position is in attack range */
        bool IsInRange(GridCell currentPosition, GridCell targetPosition);

        /** Called if a launched projectile collided */
        void ProjectileCollided();

        void OnDead();

    }
}