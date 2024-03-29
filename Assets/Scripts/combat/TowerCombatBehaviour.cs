﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora;
using Pandora.Engine;

namespace Pandora.Combat
{
    public class TowerCombatBehaviour : MonoBehaviour, CombatBehaviour, EngineBehaviour
    {
        public MapComponent map;
        public GameObject projectile;
        public string ComponentName
        {
            get => "TowerCombatBehaviour";
        }

        public Vector2 aggroBoxOrigin
        {
            get
            {
                return MapComponent.Instance.GetTowerAggroBoxOrigin(towerPosition.EngineTowerPosition).Value;
            }
        }

        public Vector2 aggroBoxEnd
        {
            get
            {
                return MapComponent.Instance.GetTowerAggroBoxEnd(towerPosition.EngineTowerPosition).Value;
            }
        }

        public bool isAttacking { get; private set; }
        public bool isMiddle = false;

        public CombatType combatType
        {
            get
            {
                return CombatType.Ranged;
            }
        }
        public int damage = 3;
        public uint cooldownMs = 300;
        public GameObject Balista;
        Animator balistaAnimator;
        public string AttackingStateName = "Attacking";
        public bool IsDisabled { get; set; } = false;

        TowerTeamComponent teamComponent;
        Vector2 worldTowerPosition
        {
            get
            {
                return map.GridCellToWorldPosition(GetComponent<TowerPositionComponent>().TowerCell);
            }
        }

        public bool isFrontTowerDestroyed = false;
        public GameObject middleTower = null;

        public GameObject CurrentTarget;
        LifeComponent targetLifeComponent;
        int aggroBoxHeight
        {
            get
            {
                return (int)(aggroBoxEnd.y - aggroBoxOrigin.y);
            }
        }

        int aggroBoxWidth
        {
            get
            {
                return (int)(aggroBoxEnd.x - aggroBoxOrigin.x);
            }
        }

        uint lastAttackTimeLapse = 0;
        TowerPositionComponent towerPosition;
        EngineComponent engineComponent;
        ProjectilePositionFixer projectilePositionFixer;


        /** Begins attacking an enemy */
        void Awake()
        {
            teamComponent = GetComponent<TowerTeamComponent>();
            towerPosition = GetComponent<TowerPositionComponent>();
            engineComponent = GetComponent<EngineComponent>();

            balistaAnimator = Balista?.GetComponent<Animator>();

            projectilePositionFixer = GetComponent<ProjectilePositionFixer>();

            if (balistaAnimator != null)
            {
                balistaAnimator.speed = 0;

                var direction = (towerPosition.WorldTowerPosition.IsTop()) ? Vector2.down : Vector2.up;

                balistaAnimator.SetFloat("BlendX", direction.x);
                balistaAnimator.SetFloat("BlendY", direction.y);
            }
        }

        public void TickUpdate(uint lapsed)
        {
            if (GetComponent<LifeComponent>().IsDead) return; // you dead man

            if (isMiddle && !isFrontTowerDestroyed) return;

            var isTargetInAggroBox = false;
            var isTargetDead = false;

            if (targetLifeComponent != null)
            {
                var targetCell = targetLifeComponent.GetComponent<EngineComponent>().Entity.GetCurrentCell();

                isTargetInAggroBox =
                    targetCell.vector.x >= aggroBoxOrigin.x &&
                    targetCell.vector.x <= aggroBoxOrigin.x + aggroBoxWidth &&
                    targetCell.vector.y >= aggroBoxOrigin.y &&
                    targetCell.vector.y <= aggroBoxOrigin.y + aggroBoxHeight;

                isTargetDead = targetLifeComponent.IsDead;
            }


            if (CurrentTarget == null)
            {
                var units = map.GetUnitsInRect(aggroBoxOrigin, aggroBoxWidth, aggroBoxHeight);

                int? closestDistance = null;
                GameObject closestUnit = null;

                foreach (var unit in units)
                {
                    var team = unit.GetComponent<TeamComponent>().Team;

                    var shouldAttack = teamComponent.EngineTeam != team;

                    if (!shouldAttack) continue;

                    var distance = engineComponent.Engine.SquaredDistance(
                        engineComponent.Entity.Position, unit.GetComponent<EngineComponent>().Entity.Position
                    );

                    if (closestDistance == null || closestDistance > distance)
                    {
                        closestDistance = distance;
                        closestUnit = unit;

                        Logger.Debug($"Closest unit is {unit}");
                    }
                }

                if (closestUnit != null)
                {
                    Logger.Debug($"Attacking {closestUnit} - {closestUnit.GetComponent<LifeComponent>()}");

                    CurrentTarget = closestUnit;
                    targetLifeComponent = CurrentTarget.GetComponent<LifeComponent>();
                }
            }
            else if (isTargetDead || !isTargetInAggroBox)
            {
                StopAttacking();
            }
            else if (targetLifeComponent != null)
            { // if not dead, attack once cooldown is over
                lastAttackTimeLapse += lapsed;

                if (balistaAnimator != null)
                {
                    var animationPercent = (float)lastAttackTimeLapse / cooldownMs;

                    balistaAnimator.Play(AttackingStateName, 0, animationPercent);

                    var direction = ((Vector2)CurrentTarget.transform.position - (Vector2)Balista.transform.position).normalized;

                    balistaAnimator.SetFloat("BlendX", direction.x);
                    balistaAnimator.SetFloat("BlendY", direction.y);
                }

                Logger.Debug($"Time lapse is {lastAttackTimeLapse} {Time.time}");

                if (lastAttackTimeLapse >= cooldownMs)
                {
                    lastAttackTimeLapse = 0;

                    AttackEnemy(new Enemy(CurrentTarget), 0);
                }
            }
        }

        public void AttackEnemy(Enemy target, uint timeLapse)
        {
            if (CurrentTarget == null || IsDisabled) return;

            Logger.Debug("Lapse: firing");
            Logger.Debug($"Attacking {target} - {isMiddle} - {targetLifeComponent}");

            isAttacking = true;

            var towerEntity = GetComponent<TowerPositionComponent>().TowerEntity;
            Vector2Int projectilePosition;

            if (projectilePositionFixer != null)
            {
                var projectileDirection = projectilePositionFixer.CalculateDirection(towerEntity, target.enemyEntity);
                projectilePosition = projectilePositionFixer.CalculateTowerProjectilePosition(towerEntity.Position, MapComponent.Instance.engine, projectileDirection);
            }
            else
            {
                projectilePosition = towerEntity.Position;
            }

            var projectileWorldPosition = MapComponent.Instance.engine.PhysicsToMapWorld(projectilePosition);

            var projectileObject = projectilePositionFixer.InstantiateHiddenProjectile(
                projectile, projectileWorldPosition, Quaternion.identity
            );

            var projectileBehaviour = projectileObject.GetComponent<ProjectileBehaviour>();

            var rotationDegrees =
                (projectileBehaviour is SimpleProjectileBehaviour simpleBehaviour) ? simpleBehaviour.StartRotationDegrees : 0;

            var direction = (target.enemy.transform.position - gameObject.transform.position).normalized;
            var angle = Vector2.SignedAngle(Vector2.up, direction) + rotationDegrees;

            Logger.Debug($"Angling projectiles at {angle}");

            // rotate the projectile towards the target
            projectileObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            projectileBehaviour.target = target;
            projectileBehaviour.parent = gameObject;
            projectileBehaviour.originalPrefab = projectile;
            projectileBehaviour.map = map;

            var epoch = System.DateTime.MinValue;
            var timestamp = epoch.AddSeconds((int)towerPosition.EngineTowerPosition * 10);
            var projectileEngineEntity = map.engine.AddEntity(projectileObject, projectileBehaviour.Speed, projectilePosition, false, timestamp);

            projectileEngineEntity.CollisionCallback = projectileBehaviour as CollisionCallback;

            projectileEngineEntity.SetTarget(target.enemyEntity);

            Logger.Debug($"Spawning projectile targeting {target} - Setting map {map}");

            var engineComponent = projectileObject.AddComponent<EngineComponent>();

            engineComponent.Entity = projectileEngineEntity;

            projectileObject.GetComponent<ProjectileBehaviour>().Init();
        }

        /** Stops attacking an enemy */
        public void StopAttacking()
        {
            Logger.Debug($"Attacking must stop - {isMiddle}");

            CurrentTarget = null;
            targetLifeComponent = null;
            isAttacking = false;

            balistaAnimator?.Play(AttackingStateName, 0, 0f);
        }

        /** Called if a launched projectile collided */
        public void ProjectileCollided(Enemy target)
        {
            Logger.Debug($"Assigning damage {targetLifeComponent} {CurrentTarget}");

            if (targetLifeComponent != null)
                targetLifeComponent.AssignDamage(damage, new TowerBaseAttack(gameObject));
        }

        public void OnDead()
        {
            if (middleTower != null)
            {
                Logger.Debug("Enabling middle tower");

                middleTower.GetComponent<TowerCombatBehaviour>().isFrontTowerDestroyed = true;
            }
        }

        public bool IsInAggroRange(Enemy enemy)
        {
            return false; // aggro is managed by TickUpdate
        }

        public bool IsInAggroRange(GridCell cell)
        {
            return false; // aggro is managed by TickUpdate
        }

        public bool IsInAttackRange(Enemy enemy)
        {
            return false; // attack is managed by TickUpdate
        }
    }
}