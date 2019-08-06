using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora;
using Pandora.Movement;

namespace Pandora.Combat
{

    public class RangedCombatBehaviour : MonoBehaviour, CombatBehaviour
    {
        float damage = 10f;
        Enemy target = null;
        public bool isAttacking { get; private set; } = false;
        public GameObject projectile;

        public CombatType combatType
        {
            get
            {
                return CombatType.Ranged;
            }
        }

        public bool IsInRange(GridCell currentPosition, GridCell targetPosition)
        {
            return Vector2.Distance(currentPosition.vector, targetPosition.vector) <= 11f;
        }

        /** Returns true if enemy has died */
        public void AttackEnemy(Enemy target)
        {
            if (!isAttacking)
            {
                this.target = target;

                var animator = GetComponent<Animator>();

                animator.SetBool("Attacking", true);

                isAttacking = true;
            }
        }

        public void StopAttacking()
        {
            isAttacking = false;
            target = null;

            var animator = GetComponent<Animator>();

            animator.SetBool("Attacking", false);
        }

        /** This method is called by an animation event */
        public void SpawnProjectile()
        {
            if (target == null) return;

            Debug.Log("Spawning projectile");

            var projectileObject = Instantiate(projectile, transform.position, Quaternion.identity);

            projectileObject.GetComponent<ProjectileBehaviour>().target = target;
            projectileObject.GetComponent<ProjectileBehaviour>().parent = gameObject;

            var map = GetComponent<MovementComponent>().map;

            var lifeComponent = target.enemy.GetComponent<LifeComponent>();

            if (lifeComponent.lifeValue - damage <= 0) // if the projectile kills the target on hit, stop attacking the target now
            {
                StopAttacking();
            }
        }

        public void ProjectileCollided() {
            var lifeComponent = target.enemy.GetComponent<LifeComponent>();

            lifeComponent.AssignDamage(damage);
        }

        public void OnDead() {}
    }
}