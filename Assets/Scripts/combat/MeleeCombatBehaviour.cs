﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Engine;

namespace Pandora.Combat
{

    public class MeleeCombatBehaviour : MonoBehaviour, CombatBehaviour
    {
        public float damage = 10f;
        GameObject target = null;
        uint timeSinceLastDamage = 0; // ms
        bool isBackswinging = false;
        public int attackCooldownMs = 500, backswingMs = 400;
        public bool isAttacking { get; private set; } = false;
        public string animationStateName;
        public int AggroRangeCells = 3, AttackRangeEngineUnits = 200;
    
        /// <summary>Multiplier applied for the next attack</summary>
        public float? NextAttackMultiplier = null;

        public CombatType combatType
        {
            get
            {
                return CombatType.Melee;
            }
        }

        /** Returns true if enemy has died */
        public void AttackEnemy(Enemy target, uint timeLapse)
        {

            var animator = GetComponent<Animator>();

            if (!isAttacking)
            {
                this.target = target.enemy;

                animator.SetBool("Attacking", true);
                animator.speed = 0;

                isAttacking = true;
            }

            animator.Play(animationStateName, 0, timeSinceLastDamage / (float) (attackCooldownMs + backswingMs));

            timeSinceLastDamage += timeLapse;

            if (timeSinceLastDamage >= attackCooldownMs && !isBackswinging) {
                MeleeAssignDamage();

                isBackswinging = true;
            }

            if (timeSinceLastDamage >= attackCooldownMs + backswingMs) {
                timeSinceLastDamage = 0;
                isBackswinging = false;
            }
        }

        public void StopAttacking()
        {
            isAttacking = false;
            target = null;

            Debug.Log($"Combat: Stop attacking {GetComponent<TeamComponent>().team}");

            var animator = GetComponent<Animator>();

            animator.SetBool("Attacking", false);
            animator.speed = 1;
        }

        public void MeleeAssignDamage()
        {
            if (target == null)
                return;

            Debug.Log($"Combat: Dealing damage {GetComponent<TeamComponent>().team}");

            var lifeComponent = target.GetComponent<LifeComponent>();

            var assignedDamage = NextAttackMultiplier.HasValue ? damage * NextAttackMultiplier.Value : damage;

            lifeComponent.AssignDamage(assignedDamage);

            NextAttackMultiplier = null;
        }

        // Do nothing, we don't have projectiles
        public void ProjectileCollided() {
        }

        public void OnDead() {}

        public bool IsInAggroRange(Enemy enemy)
        {
            var engineComponent = GetComponent<EngineComponent>();
            var engine = engineComponent.Engine;

            return engine.IsInRangeCells(engineComponent.Entity, enemy.enemyEntity, AggroRangeCells);
        }

        public bool IsInAttackRange(Enemy enemy)
        {
            var engineComponent = GetComponent<EngineComponent>();
            var engine = engineComponent.Engine;

            return engine.IsInRange(engineComponent.Entity, enemy.enemyEntity, AttackRangeEngineUnits);
        }
    }
}