using System;
using System.Collections.Generic;
using UnityEngine;
using Pandora;
using Pandora.Combat;
using Pandora.Engine;

namespace Pandora.Combat
{
    public class AreaCombatBehaviour : MonoBehaviour, CombatBehaviour
    {
        bool isBackswinging = false;
        Enemy target = null;
        uint timeSinceLastProjectile = 0;
        float shotDirectionThreshold = 0.5f;
        int vfxTicksElapsed = 0;
        uint vfxTotalTimeElapsed = 0;
        int timeSinceLastDamages = 0;
        ParticleSystem[] particles = new ParticleSystem[0];
        Queue<Dictionary<GameObject, int>> delayedDamages = new Queue<Dictionary<GameObject, int>>();
        EngineComponent engineComponent;
        AreaDamage areaDamage;
        bool isVfxRunning = false;
        public float VfxAnimationStartTime = 0f, VfxEndAnimationTime = 0f;
        public int VfxAnimationDelay = 0;
        public int Damage = 10;
        public bool isAttacking { get; private set; } = false;
        public bool IsDisabled { get; set; } = false;
        public int AggroRangeCells = 3;
        public int AttackRangeEngineUnits = 2000;
        public string AnimationStateName;
        public int AttackCooldownMs = 3000, BackswingMs = 200;
        public GameObject CombatVFX;
        public uint DamagesDelay = 0;
        public CombatType combatType
        {
            get
            {
                return CombatType.Ranged;
            }
        }

        public void Awake()
        {
            if (CombatVFX != null)
            {
                particles = CombatVFX.GetComponentsInChildren<ParticleSystem>();
            }

            engineComponent = GetComponent<EngineComponent>();
            areaDamage = GetComponent<AreaDamage>();
        }

        public void StopAttacking()
        {
            isAttacking = false;
            target = null;

            isVfxRunning = false;
            vfxTicksElapsed = 0;
            vfxTotalTimeElapsed = 0;

            PlayVFXAtTime(0f);

            var animator = GetComponent<Animator>();

            animator.SetBool("Attacking", false);
        }

        public void AttackEnemy(Enemy target, uint timeLapse)
        {
            if (IsDisabled) return;

            var animator = GetComponent<Animator>();

            var cappedBackswingMs = Math.Max(1, BackswingMs);
            var cappedAttackCooldownMs = Math.Max(1, AttackCooldownMs);

            if (!isAttacking)
            {
                this.target = target;

                animator.SetBool("Attacking", true);
                animator.speed = 0;

                isAttacking = true;
            }

            var direction = ((Vector2)target.enemy.transform.position - (Vector2)transform.position).normalized;
            var computedDirection = GetShotDirection(direction);

            var vfxFixers = CombatVFX.GetComponentsInChildren<CombatVFXFixer>();

            foreach (var vfxFixer in vfxFixers)
            {
                vfxFixer.FixVFX(computedDirection, direction);
            }

            animator.SetFloat("BlendX", computedDirection.x);
            animator.SetFloat("BlendY", computedDirection.y);

            animator.Play(AnimationStateName, 0, timeSinceLastProjectile / (float)(cappedAttackCooldownMs + cappedBackswingMs));

            timeSinceLastProjectile += timeLapse;

            if (timeSinceLastProjectile >= cappedAttackCooldownMs && !isBackswinging)
            {
                vfxTicksElapsed = 0;
                vfxTotalTimeElapsed = 0;
                isVfxRunning = true;

                // Apply damage
                var areaDamages = areaDamage.CalculateAreaDamages(target);
                delayedDamages.Enqueue(areaDamages);

                isBackswinging = true;
            }
            else if (timeSinceLastProjectile >= cappedAttackCooldownMs + cappedBackswingMs)
            {
                timeSinceLastProjectile = 0;

                isBackswinging = false;
            }
        }

        public void ProjectileCollided(Enemy target)
        {
            var lifeComponent = target?.enemy.GetComponent<LifeComponent>();
            lifeComponent?.AssignDamage(Damage, new BaseAttack(gameObject));
        }

        public void OnDead() { }

        public bool IsInAggroRange(Enemy enemy)
        {
            var engine = engineComponent.Engine;

            return engine.IsInHitboxRangeCells(engineComponent.Entity, enemy.enemyEntity, AggroRangeCells);
        }

        public bool IsInAggroRange(GridCell cell)
        {
            var engine = engineComponent.Engine;

            return Vector2.Distance(cell.vector, engineComponent.Entity.GetCurrentCell().vector) < AggroRangeCells;
        }

        public bool IsInAttackRange(Enemy enemy)
        {
            var engine = engineComponent.Engine;

            return engine.IsInHitboxRange(engineComponent.Entity, enemy.enemyEntity, AttackRangeEngineUnits);
        }

        public void PlayVFXNextFrame(uint timeLapsed)
        {
            if (CombatVFX == null || !isVfxRunning) return;

            vfxTicksElapsed++;
            vfxTotalTimeElapsed += timeLapsed;

            float animationPercent = (float)vfxTotalTimeElapsed / AttackCooldownMs;

            var time =
                vfxTotalTimeElapsed > VfxAnimationDelay
                    ? VfxAnimationStartTime + (animationPercent * (VfxEndAnimationTime - VfxAnimationStartTime))
                    : 0;

            PlayVFXAtTime(time);
        }

        public void ApplyDamages(uint timeLapsed)
        {
            if (delayedDamages.Count <= 0) return;

            timeSinceLastDamages += (int)timeLapsed;

            if (timeSinceLastDamages < DamagesDelay) return;

            var damagesToApply = delayedDamages.Dequeue();

            foreach (var damage in damagesToApply)
            {
                var lifeComponent = damage.Key.GetComponent<LifeComponent>();

                Logger.Debug($"Damaging entity with damage {damage.Value} | {timeLapsed}");

                if (lifeComponent)
                    lifeComponent.AssignDamage(damage.Value, new BaseAttack(gameObject));
            }

            timeSinceLastDamages = 0;
        }

        Vector2Int GetShotDirection(Vector2 enemyDirection)
        {
            var direction = new Vector2Int();

            if (enemyDirection != null)
            {
                direction.x = enemyDirection.x > shotDirectionThreshold ? 1 : enemyDirection.x < -shotDirectionThreshold ? -1 : 0;
                direction.y = enemyDirection.y > shotDirectionThreshold ? 1 : enemyDirection.y < -shotDirectionThreshold ? -1 : 0;
            }

            return direction;
        }

        void PlayVFXAtTime(float time)
        {
            foreach (var particle in particles)
            {
                particle.Pause();
                particle.Simulate(time);
            }
        }
    }
}