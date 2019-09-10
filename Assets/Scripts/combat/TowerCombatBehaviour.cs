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
        public string ComponentName {
            get {
                return "TowerCombatBehaviour";
            }
        }

        public Vector2 aggroBoxOrigin {
            get {
                return MapComponent.Instance.GetTowerAggroBoxOrigin(towerPosition.EngineTowerPosition).Value;
            }
        }

        public Vector2 aggroBoxEnd {
            get {
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
        public int range = 5;
        public float damage = 3f;
        public uint cooldownMs = 300;
        public bool isOpponent = false;

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

        GameObject currentTarget;
        LifeComponent targetLifeComponent;
        int aggroBoxHeight {
            get {
                return (int)(aggroBoxEnd.y - aggroBoxOrigin.y);
            }
        }

        int aggroBoxWidth {
            get {
                return (int)(aggroBoxEnd.x - aggroBoxOrigin.x);
            }
        }

        uint lastAttackTimeLapse = 0;
        TowerPositionComponent towerPosition;


        /** Begins attacking an enemy */
        void Awake()
        {
            teamComponent = GetComponent<TowerTeamComponent>();

            towerPosition = GetComponent<TowerPositionComponent>();
        }

        public void TickUpdate(uint lapsed)
        {
            if (GetComponent<LifeComponent>().isDead) return; // you dead man

            if (isMiddle && !isFrontTowerDestroyed) return;

            if (currentTarget == null)
            {
                var units = map.GetUnitsInRect(aggroBoxOrigin, aggroBoxWidth, aggroBoxHeight);

                float? closestDistance = null;
                GameObject closestUnit = null;

                foreach (var unit in units)
                {
                    var team = unit.GetComponent<TeamComponent>().team;

                    var shouldAttack = teamComponent.engineTeam != team;

                    if (!shouldAttack) continue;

                    var distance = Vector2.Distance(
                        worldTowerPosition, unit.transform.position
                    );

                    if (closestDistance == null || closestDistance > distance)
                    {
                        closestDistance = distance;
                        closestUnit = unit;

                        Debug.Log($"Closest unit is {unit}");
                    }
                }

                if (closestUnit != null)
                {
                    Debug.Log($"Attacking {closestUnit} - {closestUnit.GetComponent<LifeComponent>()}");

                    currentTarget = closestUnit;
                    targetLifeComponent = currentTarget.GetComponent<LifeComponent>();

                    AttackEnemy(new Enemy(currentTarget), 0);
                }
            }
            else if (targetLifeComponent != null && targetLifeComponent.isDead)
            {
                StopAttacking();
            }
            else if (targetLifeComponent != null)
            { // if not dead, attack once cooldown is over
                lastAttackTimeLapse += lapsed;

                if (lastAttackTimeLapse >= cooldownMs)
                {
                    lastAttackTimeLapse = 0;

                    AttackEnemy(new Enemy(currentTarget), 0);
                }
            }
        }

        public void AttackEnemy(Enemy target, uint timeLapse)
        {
            if (currentTarget == null) return;

            Debug.Log($"Attacking {target} - {isMiddle} - {targetLifeComponent}");

            isAttacking = true;

            var projectileObject = Instantiate(projectile, worldTowerPosition, Quaternion.identity);
            var projectileBehaviour = projectileObject.GetComponent<ProjectileBehaviour>();

            projectileObject.GetComponent<ProjectileBehaviour>().target = target;
            projectileObject.GetComponent<ProjectileBehaviour>().parent = gameObject;
            projectileObject.GetComponent<ProjectileBehaviour>().map = map;
        }

        /** Stops attacking an enemy */
        public void StopAttacking()
        {
            Debug.Log($"Attacking must stop - {isMiddle}");

            currentTarget = null;
            targetLifeComponent = null;
            isAttacking = false;
        }

        /** Called if a launched projectile collided */
        public void ProjectileCollided()
        {
            Debug.Log($"Assigning damage {targetLifeComponent} {currentTarget}");

            if (targetLifeComponent != null)
                targetLifeComponent.AssignDamage(damage);
        }

        public void OnDead()
        {
            if (middleTower != null)
            {
                Debug.Log("Enabling middle tower");

                middleTower.GetComponent<TowerCombatBehaviour>().isFrontTowerDestroyed = true;
            }
        }
    }
}