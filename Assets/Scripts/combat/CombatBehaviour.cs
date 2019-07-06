using CRclone;
using UnityEngine;

namespace CRclone.Combat {
    interface CombatBehaviour {
        /** True if the unit is currently in combat */
        bool isAttacking { get; }

        /** Begins attacking an enemy */
        void AttackEnemy(Enemy target);
        /** Stops attacking an enemy */
        void StopAttacking();
        /** Checks whether a position is in attack range */
        bool IsInRange(Vector2 currentPosition, Vector2 targetPosition);
    }
}