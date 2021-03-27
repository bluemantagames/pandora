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
        public bool IsDisabled { get; set; } = false;
        public int AggroRangeCells = 3, AttackRangeEngineUnits = 2000;
        public GameObject[] EffectObjects;
        public int ProjectileAdjustmentX = 0;
        public int ProjectileAdjustmentY = 0;

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
            if (IsDisabled) return;

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

            var direction = ((Vector2)target.enemy.transform.position - (Vector2)transform.position).normalized;

            animator.SetFloat("BlendX", direction.x);
            animator.SetFloat("BlendY", direction.y);

            animator.Play(animationStateName, 0, timeSinceLastProjectile / (float)(cappedAttackCooldownMs + cappedBackswingMs));

            timeSinceLastProjectile += timeLapse;

            if (timeSinceLastProjectile >= cappedAttackCooldownMs && !isBackswinging)
            {
                SpawnProjectile();

                isBackswinging = true;
            }
            else if (timeSinceLastProjectile >= cappedAttackCooldownMs + cappedBackswingMs)
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

            var map = MapComponent.Instance;

            var unitEngineComponent = GetComponent<EngineComponent>();
            var engine = unitEngineComponent.Engine;
            var engineEntity = unitEngineComponent.Entity;
            var rawDirection = target.enemyEntity.Position - engineEntity.Position;

            // FIXME: This should be optimized or abstracted.
            // At the moment there are problem with the direct up-down-left-right
            // directions since they should be ~precise
            var direction = new Vector2Int(
                rawDirection.x > 0 ? 1 : rawDirection.x < 0 ? -1 : 0,
                rawDirection.y > 0 ? 1 : rawDirection.y < 0 ? -1 : 0
            );

            var projectilePosition = CalculateProjectilePosition(engineEntity, engine, direction);

            var projectileObject = Instantiate(projectile, transform.position, Quaternion.identity);
            var projectileBehaviour = projectileObject.GetComponent<ProjectileBehaviour>();

            projectileBehaviour.target = target;
            projectileBehaviour.parent = gameObject;
            projectileBehaviour.originalPrefab = projectile;
            projectileBehaviour.map = map;

            var timestamp = engineEntity.Timestamp.AddMilliseconds(map.engine.TotalElapsed);

            var projectileEngineEntity = map.engine.AddEntity(projectileObject, projectileBehaviour.Speed, projectilePosition, false, timestamp);

            projectileEngineEntity.CollisionCallback = projectileBehaviour as CollisionCallback;

            projectileEngineEntity.SetTarget(target.enemyEntity);

            Logger.Debug($"Spawning projectile targeting {target} - Setting map {map}");

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

            lifeComponent?.AssignDamage(Damage, new BaseAttack(gameObject));

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

        private Vector2Int CalculateProjectilePosition(EngineEntity unitEntity, PandoraEngine engine, Vector2Int direction)
        {
            var basePosition = unitEntity.Position;
            var computedPosition = new Vector2Int(basePosition.x + ProjectileAdjustmentX, basePosition.y + ProjectileAdjustmentY);
            var rotatedPosition = engine.RotateFigureByDirection(new List<Vector2Int>() { computedPosition }, basePosition, direction)[0];

            Logger.Debug($"Calculated projectile position: ({rotatedPosition.x}, {rotatedPosition.y}) for direction {direction}");

            return rotatedPosition;
        }
    }
}