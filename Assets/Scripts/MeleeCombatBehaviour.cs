namespace CRclone
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MeleeCombatBehaviour : MonoBehaviour
    {
        float damage = 10f;
        LifeComponent target = null;

        public bool IsInRange(Vector2 currentPosition, Vector2 targetPosition) {
            return Vector2.Distance(currentPosition, targetPosition) <= 1f;
        }

        /** Returns true if enemy has died */
        public bool AttackEnemy(Enemy target)
        {
            return false;
        }

        /** This method is called by an animation event */
        public void MeleeAssignDamage()
        {
            if (target == null)
                return;

            target.AssignDamage(damage);
        }
    }
}