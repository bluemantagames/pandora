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
    }
}