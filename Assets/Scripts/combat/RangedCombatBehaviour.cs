using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora;
using Pandora.Movement;
using Pandora.Engine;
using System;

namespace Pandora.Combat
{

    public class RangedCombatBehaviour : MonoBehaviour, CombatBehaviour
    {
        public int Damage = 10;
        Enemy target = null;
        public bool isAttacking { get; private set; } = false;
        public int attackCooldownMs = 600, backswingMs = 200;
        public GameObject projectile;
        public string animationStateName;
        uint timeSinceLastProjectile = 0; // ms
        bool isBackswinging = false;
        public int AggroRangeCells = 3, AttackRangeEngineUnits = 2000;
        public GameObject[] EffectObjects;

        public CombatType combatType
        {
            get
            {
                return CombatType.Ranged;
            }
        }

        /** Returns true if enemy has died */
        public void AttackEnemy(Enemy target, uint timeLapse)
        {
            var animator = GetComponent<Animator>();

            var cappedBackswingMs = Math.Max(1, backswingMs);
            var cappedAttackCooldownMs = Math.Max(1, attackCooldownMs);

            if (!isAttacking)
            {
                this.target = target;

                animator.SetBool("Attacking", true);
                animator.speed = 0;

                isAttacking = true;
            }

            animator.Play(animationStateName, 0, timeSinceLastProjectile / (float)(cappedAttackCooldownMs + cappedBackswingMs));

            timeSinceLastProjectile += timeLapse;

            if (timeSinceLastProjectile >= cappedAttackCooldownMs && !isBackswinging)
            {
                SpawnProjectile();

                isBackswinging = true;
            } else if (timeSinceLastProjectile >= cappedAttackCooldownMs + cappedBackswingMs)
            {
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
            var projectileBehaviour = projectileObject.GetComponent<ProjectileBehaviour>();

            var map = MapComponent.Instance;

            var engineEntity = GetComponent<EngineComponent>().Entity;

            var timestamp = engineEntity.Timestamp.AddMilliseconds(map.engine.TotalElapsed);

            var projectileEngineEntity = map.engine.AddEntity(projectileObject, projectileBehaviour.Speed, engineEntity.Position, false, timestamp);

            projectileEngineEntity.CollisionCallback = projectileBehaviour as CollisionCallback;

            projectileEngineEntity.SetTarget(target.enemyEntity);

            Logger.Debug($"Spawning projectile targeting {target} - Setting map {map}");

            projectileObject.GetComponent<ProjectileBehaviour>().target = target;
            projectileObject.GetComponent<ProjectileBehaviour>().parent = gameObject;
            projectileObject.GetComponent<ProjectileBehaviour>().map = map;

            var engineComponent = projectileObject.AddComponent<EngineComponent>();

            engineComponent.Entity = projectileEngineEntity;

            projectileObject.GetComponent<ProjectileBehaviour>().Init();

            var lifeComponent = target.enemy.GetComponent<LifeComponent>();

            if (lifeComponent.lifeValue - Damage <= 0) // if the projectile kills the target on hit, stop attacking the target now
            {
                StopAttacking();
            }
        }

        public void ProjectileCollided(Enemy target)
        {
            var lifeComponent = target?.enemy.GetComponent<LifeComponent>();

            lifeComponent?.AssignDamage(Damage);

            foreach (var effectObject in EffectObjects)
            {
                var effect = effectObject.GetComponent<Effect>();

                effect.Apply(gameObject, target.enemy);
            }
        }

        public void OnDead() { }

        public bool IsInAggroRange(Enemy enemy)
        {
            var engineComponent = GetComponent<EngineComponent>();
            var engine = engineComponent.Engine;

            return engine.IsInHitboxRangeCells(engineComponent.Entity, enemy.enemyEntity, AggroRangeCells);
        }

        public bool IsInAggroRange(GridCell cell)
        {
            var engineComponent = GetComponent<EngineComponent>();
            var engine = engineComponent.Engine;

            return Vector2.Distance(cell.vector, engineComponent.Entity.GetCurrentCell().vector) < AggroRangeCells;
        }

        public bool IsInAttackRange(Enemy enemy)
        {
            var engineComponent = GetComponent<EngineComponent>();
            var engine = engineComponent.Engine;

            return engine.IsInHitboxRange(engineComponent.Entity, enemy.enemyEntity, AttackRangeEngineUnits);
        }
    }
}