using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Engine;
using System;

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
        public bool IsDisabled { get; set; } = false;
        public string animationStateName = "Attacking";
        public int AggroRangeCells = 3, AttackRangeEngineUnits = 0;
        public List<Effect> Effects = new List<Effect> { };
        public GameObject MultipliedVFX;

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
            if (IsDisabled) return;

            var animator = GetComponent<Animator>();

            var cappedBackswingMs = Math.Max(1, backswingMs);
            var cappedAttackCooldownMs = Math.Max(1, attackCooldownMs);

            if (!isAttacking)
            {
                this.target = target.enemy;

                animator.SetBool("Attacking", true);
                animator.speed = 0;

                isAttacking = true;
            }

            animator.Play(animationStateName, 0, timeSinceLastDamage / (float)(cappedAttackCooldownMs + cappedBackswingMs));

            timeSinceLastDamage += timeLapse;

            if (timeSinceLastDamage >= cappedAttackCooldownMs && !isBackswinging)
            {
                MeleeAssignDamage();

                isBackswinging = true;
            }

            if (timeSinceLastDamage >= cappedAttackCooldownMs + cappedBackswingMs)
            {
                timeSinceLastDamage = 0;
                isBackswinging = false;
            }
        }

        public void StopAttacking()
        {
            isAttacking = false;
            target = null;

            Logger.Debug($"Combat: Stop attacking {GetComponent<TeamComponent>().Team}");

            var animator = GetComponent<Animator>();

            animator.SetBool("Attacking", false);
            animator.speed = 1;
        }

        public void MeleeAssignDamage()
        {
            if (target == null)
                return;

            Logger.Debug($"Combat: Dealing damage {GetComponent<TeamComponent>().Team}");

            var lifeComponent = target.GetComponent<LifeComponent>();

            var inflictedDamage = damage;

            if (NextAttackMultiplier.HasValue)
            {
                inflictedDamage = NextAttackMultiplier.Value * damage;

                if (MultipliedVFX != null)
                {
                    var vfx = Instantiate(MultipliedVFX, target.transform.position, MultipliedVFX.transform.rotation);

                    var callback = vfx.GetComponent<CombatVFXCallback>();

                    if (callback != null) callback.Target = target;

                    vfx.GetComponent<ParticleSystem>()?.Play();
                }
            }

            lifeComponent.AssignDamage(inflictedDamage, new BaseAttack(gameObject));

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

        public void ChangeDamage(int newDamege)
        {
            damage = newDamege;
        }
    }
}