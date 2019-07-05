namespace CRclone
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MeleeCombatBehaviour : MonoBehaviour
    {
        float damage = 10f;
        GameObject target = null;
        public bool isAttacking = false;

        public bool IsInRange(Vector2 currentPosition, Vector2 targetPosition) {
            return Vector2.Distance(currentPosition, targetPosition) <= 2f;
        }

        /** Returns true if enemy has died */
        public void AttackEnemy(Enemy target)
        {
            if (!isAttacking) {
                this.target = target.enemy;

                var animator = GetComponent<Animator>();

                animator.SetBool("Attacking", true);

                isAttacking = true;
            }
        }

        public void StopAttacking() {
            isAttacking = false;
            target = null;

            var animator = GetComponent<Animator>();

            animator.SetBool("Attacking", false);
        }

        /** This method is called by an animation event */
        public void MeleeAssignDamage()
        {
            if (target == null)
                return;

            var lifeComponent = target.GetComponent<LifeComponent>();

            lifeComponent.AssignDamage(damage);

            if (lifeComponent.lifeValue <= 0) {
                StopAttacking();
            }
        }
    }
}