using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pandora.Combat
{

    public class MeleeCombatBehaviour : MonoBehaviour, CombatBehaviour
    {
        public float damage = 10f;
        GameObject target = null;
        public bool isAttacking { get; private set; } = false;

        public CombatType combatType
        {
            get
            {
                return CombatType.Melee;
            }
        }

        public bool IsInRange(GridCell currentPosition, GridCell targetPosition)
        {
            return Vector2.Distance(currentPosition.vector, targetPosition.vector) <= 2f;
        }

        /** Returns true if enemy has died */
        public void AttackEnemy(Enemy target)
        {
            if (!isAttacking)
            {
                this.target = target.enemy;

                var animator = GetComponent<Animator>();

                animator.SetBool("Attacking", true);

                isAttacking = true;
            }
        }

        public void StopAttacking()
        {
            isAttacking = false;
            target = null;

            Debug.Log($"Combat: Stop attacking {GetComponent<TeamComponent>().team}");

            var animator = GetComponent<Animator>();

            animator.SetBool("Attacking", false);
        }

        /** This method is called by an animation event */
        public void MeleeAssignDamage()
        {
            if (target == null)
                return;

            Debug.Log($"Combat: Dealing damage {GetComponent<TeamComponent>().team}");

            var lifeComponent = target.GetComponent<LifeComponent>();

            lifeComponent.AssignDamage(damage);
        }

        // Do nothing, we don't have projectiles
        public void ProjectileCollided() {
        }

        public void OnDead() {}

    }
}