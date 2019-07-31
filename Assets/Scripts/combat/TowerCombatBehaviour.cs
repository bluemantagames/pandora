using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CRclone;

namespace CRclone.Combat
{
    public class TowerCombatBehaviour : MonoBehaviour, CombatBehaviour
    {
        public MapListener map;
        public GameObject projectile;
        public Vector2 aggroBoxOrigin, aggroBoxEnd; // bottom left corner

        public bool isAttacking { get; private set; }
        public CombatType combatType
        {
            get
            {
                return CombatType.Ranged;
            }
        }
        public int range = 5;
        public float damage = 3f;
        public float cooldown = 0.3f;
        public bool isOpponent = false;

        TeamComponent teamComponent;
        Vector2 worldTowerPosition { 
            get {
                return map.GridCellToWorldPosition(GetComponent<TowerPositionComponent>().position);
            }
        }

        GameObject currentTarget;
        LifeComponent targetLifeComponent;
        int aggroBoxHeight, aggroBoxWidth;
        float lastAttackTimeLapse = 0f;


        /** Begins attacking an enemy */
        void Awake()
        {
            aggroBoxHeight = (int) (aggroBoxEnd.y - aggroBoxOrigin.y);
            aggroBoxWidth = (int) (aggroBoxEnd.x - aggroBoxOrigin.x);

            teamComponent = GetComponent<TeamComponent>();
        }

        // Update is called once per frame
        void Update()
        {
            if (GetComponent<LifeComponent>().isDead) return; // you dead man

            if (currentTarget == null)
            {
                var units = map.GetUnitsInRect(aggroBoxOrigin, aggroBoxWidth, aggroBoxHeight);

                float? closestDistance = null;
                GameObject closestUnit = null;

                foreach (var unit in units)
                {
                    var team = unit.GetComponent<TeamComponent>().team;

                    if (isOpponent && team != TeamComponent.assignedTeam) continue;
                    if (!isOpponent && team == TeamComponent.assignedTeam) continue;

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
                    Debug.Log($"Attacking {closestUnit}");

                    currentTarget = closestUnit;
                    targetLifeComponent = currentTarget.GetComponent<LifeComponent>();

                    AttackEnemy(new Enemy(currentTarget));
                }
            }
            else if (targetLifeComponent != null && targetLifeComponent.isDead)
            {
                currentTarget = null;
                targetLifeComponent = null;
            }
            else if (targetLifeComponent != null)
            { // if not dead, attack once cooldown is over
                lastAttackTimeLapse += Time.deltaTime;

                if (lastAttackTimeLapse >= cooldown)
                {
                    lastAttackTimeLapse = 0f;

                    AttackEnemy(new Enemy(currentTarget));
                }
            }
        }

        public void AttackEnemy(Enemy target)
        {
            if (currentTarget == null) return;

            isAttacking = true;

            var projectileObject = Instantiate(projectile, worldTowerPosition, Quaternion.identity);

            projectileObject.GetComponent<ProjectileBehaviour>().target = target;
            projectileObject.GetComponent<ProjectileBehaviour>().parent = gameObject;
        }

        /** Stops attacking an enemy */
        public void StopAttacking()
        {
            currentTarget = null;
            targetLifeComponent = null;
            isAttacking = false;
        }

        /** Checks whether a position is in attack range, not used */
        public bool IsInRange(Vector2 currentPosition, Vector2 targetPosition)
        {
            return false;
        }

        /** Called if a launched projectile collided */
        public void ProjectileCollided()
        {
            targetLifeComponent.AssignDamage(damage);
        }

    }
}