using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Priority_Queue;
using Pandora;
using Pandora.Combat;
using UnityEngine.Profiling;
using Pandora.Engine;
using Pandora.Pool;

namespace Pandora.Movement
{
    public class MermaidMovementBehaviour : MonoBehaviour, MovementBehaviour
    {
        public MapComponent map { get; set; }
        public MovementStateEnum LastState { get; set; }

        EngineEntity entity;
        PandoraEngine engine;
        TeamComponent team;
        CombatBehaviour combatBehaviour;

        public Vector2 WalkingDirection { get; set; }

        public int MovementSpeed = 400;

        public int Speed
        {
            get => MovementSpeed;
            set => MovementSpeed = value;
        }

        void Start()
        {
            var engineComponent = GetComponent<EngineComponent>();

            entity = engineComponent.Entity;
            engine = engineComponent.Engine;

            team = GetComponent<TeamComponent>();
            combatBehaviour = GetComponent<CombatBehaviour>();

            WalkingDirection = Vector2.up;
        }

        public MovementState Move()
        {
            if (entity == null)
            {
                return new MovementState(null, MovementStateEnum.Idle);
            }

            transform.position = entity.GetWorldPosition();

            var currentPosition = entity.GetCurrentCell();

            var target = map.GetEnemy(gameObject, currentPosition, team);

            if (target.IsTower)
            {
                return new MovementState(null, MovementStateEnum.Idle);
            }

            if (combatBehaviour.IsInAttackRange(target))
            {
                entity.SetEmptyPath();

                return new MovementState(target, MovementStateEnum.EnemyApproached);
            }
            else
            {
                if (combatBehaviour.isAttacking) combatBehaviour.StopAttacking();

                entity.SetSpeed(MovementSpeed);

                var targetPosition = currentPosition;

                targetPosition.vector.x = target.enemyCell.vector.x;

                entity.SetTarget(targetPosition);

                var walkingDirection = ((Vector2)targetPosition.vector - currentPosition.vector).normalized;

                if (walkingDirection != Vector2.zero) {
                    WalkingDirection = walkingDirection;
                }

                return new MovementState(target, MovementStateEnum.TargetAcquired);
            }
        }

        public void ResetPath()
        {

        }
    }
}