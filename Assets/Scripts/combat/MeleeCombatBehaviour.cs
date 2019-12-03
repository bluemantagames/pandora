using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Engine;

namespace Pandora.Combat
{

    public class MeleeCombatBehaviour : MonoBehaviour, CombatBehaviour
    {
        public int damage = 10;
        GameObject target = null;
        uint timeSinceLastDamage = 0; // ms
        bool isBackswinging = false;
        public int attackCooldownMs = 500, backswingMs = 400;
        public bool isAttacking { get; private set; } = false;
        public string animationStateName;
        public int AggroRangeCells = 3, AttackRangeEngineUnits = 0;
        public List<Effect> Effects = new List<Effect> {};

        /// <summary>Multiplier applied for the next attack</summary>
        public int? NextAttackMultiplier = null;

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

            animator.Play(animationStateName, 0, timeSinceLastDamage / (float)(attackCooldownMs + backswingMs));

            timeSinceLastDamage += timeLapse;

            if (timeSinceLastDamage >= attackCooldownMs && !isBackswinging)
            {
                MeleeAssignDamage();

                isBackswinging = true;
            }

            if (timeSinceLastDamage >= attackCooldownMs + backswingMs)
            {
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

            foreach (var effect in Effects)
            {
                effect.Apply(gameObject, target);
            }
        }

        // Do nothing, we don't have projectiles
        public void ProjectileCollided(Enemy target)
        {
        }

        public void OnDead() { }

        public bool IsInAggroRange(Enemy enemy)
        {
            var engineComponent = GetComponent<EngineComponent>();
            var engine = engineComponent.Engine;

            return engine.IsInHitboxRangeCells(engineComponent.Entity, enemy.enemyEntity, AggroRangeCells);
        }

        public bool IsInAttackRange(Enemy enemy)
        {
            var engineComponent = GetComponent<EngineComponent>();
            var engine = engineComponent.Engine;

            return engine.IsInHitboxRange(engineComponent.Entity, enemy.enemyEntity, AttackRangeEngineUnits);
        }

        public bool IsInAggroRange(GridCell cell)
        {
            var engineComponent = GetComponent<EngineComponent>();
            var engine = engineComponent.Engine;

            return Vector2.Distance(cell.vector, engineComponent.Entity.GetCurrentCell().vector) <= AggroRangeCells;
        }
    }
}