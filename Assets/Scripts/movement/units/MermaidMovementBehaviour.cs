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
using Pandora.Combat;

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

        public int MovementSpeed = 400;

        public int Speed {
            get => MovementSpeed;
            set => MovementSpeed = value;
        }

        void Start()
        {
            var engineComponent = GetComponent<EngineComponent>();

            entity = engineComponent.Entity;
            engine = engineComponent.Engine;

            entity.Layer = Constants.SWIMMING_LAYER;

            team = GetComponent<TeamComponent>();
            combatBehaviour = GetComponent<CombatBehaviour>();
        }

        public MovementState Move()
        {
            transform.position =
                (TeamComponent.assignedTeam == TeamComponent.bottomTeam) ?
                    entity.GetWorldPosition() :
                    entity.GetFlippedWorldPosition();

            var currentPosition = entity.GetCurrentCell();

            var target = map.GetEnemy(gameObject, currentPosition, team);

            if (target.IsTower)
            {
                return new MovementState(null, MovementStateEnum.Idle);
            }

            if (combatBehaviour.IsInAttackRange(target)) {
                return new MovementState(target, MovementStateEnum.EnemyApproached);
            } else {
                var targetPosition = currentPosition;

                targetPosition.vector.x = target.enemyCell.vector.x;

                entity.SetTarget(targetPosition);

                return new MovementState(target, MovementStateEnum.TargetAcquired);
            }

        }
    }
}