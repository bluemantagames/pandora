﻿using System.Collections;
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
        public int attackCooldownMs = 600, backswingMs = 200;
        public GameObject projectile;
        public string animationStateName;
        int timeSinceLastProjectile = 0; // ms
        bool isBackswinging = true;

        public CombatType combatType
        {
            get
            {
                return CombatType.Ranged;
            }
        }

        /** Returns true if enemy has died */
        public void AttackEnemy(Enemy target, int timeLapse)
        {
            var animator = GetComponent<Animator>();

            if (!isAttacking)
            {
                this.target = target;

                animator.SetBool("Attacking", true);
                animator.speed = 0;

                isAttacking = true;
            }


            animator.Play(animationStateName, 0, timeSinceLastProjectile / 1000f);

            timeSinceLastProjectile += timeLapse;

            if (timeSinceLastProjectile >= attackCooldownMs && !isBackswinging) {
                SpawnProjectile();

                isBackswinging = true;
            }

            if (timeSinceLastProjectile >= attackCooldownMs + backswingMs) {
                timeSinceLastProjectile = 0;
                isBackswinging = false;
            }
        }

        public void StopAttacking()
        {
            isAttacking = false;
            target = null;

            var animator = GetComponent<Animator>();

            animator.SetBool("Attacking", false);
        }

        public void SpawnProjectile()
        {
            if (target == null) return;

            var projectileObject = Instantiate(projectile, transform.position, Quaternion.identity);

            var map = GetComponent<MovementComponent>().map;

            Debug.Log($"Spawning projectile - Setting map {map}");

            projectileObject.GetComponent<ProjectileBehaviour>().target = target;
            projectileObject.GetComponent<ProjectileBehaviour>().parent = gameObject;
            projectileObject.GetComponent<ProjectileBehaviour>().map = map;

            var lifeComponent = target.enemy.GetComponent<LifeComponent>();

            if (lifeComponent.lifeValue - damage <= 0) // if the projectile kills the target on hit, stop attacking the target now
            {
                StopAttacking();
            }
        }

        public void ProjectileCollided() {
            var lifeComponent = target?.enemy.GetComponent<LifeComponent>();

            lifeComponent?.AssignDamage(damage);
        }

        public void OnDead() {}
    }
}