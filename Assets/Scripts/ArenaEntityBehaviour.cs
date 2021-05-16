﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Profiling;
using Pandora.AI;
using Pandora.Combat;
using Pandora.Engine;
using System;

namespace Pandora
{
    public class ArenaEntityBehaviour : MonoBehaviour, EngineBehaviour
    {
        EntityController movementBehaviour;
        CombatBehaviour combatBehaviour;
        LifeComponent lifeComponent;
        EngineComponent engineComponent;
        TeamComponent teamComponent;
        public bool DebugMove = false;
        int animationSmoothingCount = 0, animationSmoothingThreshold = 1, directions = 12;
        Vector2? targetDirection = null;
        List<Vector2> blendTreePoints = new List<Vector2> { };

        // represents whether we are handling movement / animation or another component is doing it
        bool areWePaused = false;
        AreaCombarBehaviour areaCombatBehaviour;
        public Bounds hitbox;
        public RuntimeAnimatorController BlueController, RedController;
        public string ComponentName
        {
            get
            {
                return "UnitBehaviour";
            }
        }

        CustomSampler moveSampler;
        Animator animator;


        public string WalkingAnimationStateName = "Walking";
        public bool WalkingAnimationEnabled = false;
        public uint WalkingAnimationEngineUnits;

        float walkingAnimationTime = 0;
        string overridingAnimationName = null;
        uint overridingAnimationMsLength = 0, overridingTotalTimePassed = 0;
        Action onOverrideEnd = null;
        int? overridenSpeed = null;

        // Start is called before the first frame update
        void Awake()
        {
            movementBehaviour = GetComponent<EntityController>();
            combatBehaviour = GetComponent<CombatBehaviour>();
            lifeComponent = GetComponent<LifeComponent>();
            areaCombatBehaviour = GetComponent<AreaCombarBehaviour>();

            moveSampler = CustomSampler.Create($"Move() {gameObject.name}");
            animator = GetComponent<Animator>();
            engineComponent = GetComponent<EngineComponent>();
            teamComponent = GetComponent<TeamComponent>();

            Logger.Debug($"CombatBehaviour is {combatBehaviour}");

            SetupAnimationControllers();

            var angle = 360 / directions;

            for (var i = 0; i < directions; i++)
            {
                blendTreePoints.Add(
                    Quaternion.Euler(0f, 0f, angle * i) * Vector2.up
                );
            }
        }

        public void SetupAnimationControllers()
        {
            if (BlueController || RedController)
            {
                animator.runtimeAnimatorController =
                    (teamComponent.Team == TeamComponent.assignedTeam) ? BlueController : RedController;
            }
        }

        // This is called from PandoraEngine every tick
        public void TickUpdate(uint timeLapsed)
        {
            if (lifeComponent.IsDead || areWePaused) return; // Do nothing if dead

            if (overridingAnimationName != null)
            {
                overridingTotalTimePassed += timeLapsed;

                playAnimation((float)overridingTotalTimePassed / (float)overridingAnimationMsLength, overridingAnimationName);

                return;
            }

            moveSampler.Begin();
            var state = movementBehaviour.Move();
            moveSampler.End();

            movementBehaviour.LastState = state.state;

            if (DebugMove)
            {
                Logger.Debug($"Movement state: {state}");
            }

            if (state.state == MovementStateEnum.EnemyApproached)
            {
                combatBehaviour.AttackEnemy(state.enemy, timeLapsed);
            }
            else if (state.state != MovementStateEnum.EnemyApproached && combatBehaviour.isAttacking)
            {
                combatBehaviour.StopAttacking();

                walkingAnimationTime = 0;
            }
            else if (WalkingAnimationEnabled)
            {
                playAnimation(walkingAnimationTime, WalkingAnimationStateName);

                var timePercent = engineComponent.Entity.Speed / ((float)WalkingAnimationEngineUnits);

                walkingAnimationTime += timePercent;

                if (walkingAnimationTime >= 1f)
                {
                    walkingAnimationTime = 0f;
                }

                // Never skip the last frame
                if (walkingAnimationTime + timePercent >= 1f)
                {
                    walkingAnimationTime = 1f;
                }
            }

            if (areaCombatBehaviour != null && areaCombatBehaviour.isAttacking)
            {
                areaCombatBehaviour.PlayVFXNextFrame(timeLapsed);
                areaCombatBehaviour.ApplyDamages(timeLapsed);
            }
        }

        /// <summary>Other behaviours can use this method to stop our handling of animation and movement</summary>
        public void PauseBehaviour()
        {
            areWePaused = true;
        }

        /// <summary>Other behaviours can use this method to resume our handling after pausing it with PauseBehaviour</summary>
        public void UnpauseBehaviour()
        {
            areWePaused = false;
        }

        public void PlayAnimation(string animationStateName, uint animationMsLength, Action animationEndCallback)
        {
            overridingAnimationName = animationStateName;
            overridingAnimationMsLength = animationMsLength;
            onOverrideEnd = animationEndCallback;
            overridenSpeed = engineComponent.Entity.Speed;

            engineComponent.Entity.SetSpeedUnitsPerSecond(0);
        }

        void playAnimation(float timePercent, string animationName)
        {
            animator.speed = 0;

            Vector2? blendedPoint = null;
            float? minBlendedSquaredDistance = null;

            foreach (var point in blendTreePoints)
            {
                var blendedSquaredDistance = (point - movementBehaviour.WalkingDirection).sqrMagnitude;

                if (!minBlendedSquaredDistance.HasValue || minBlendedSquaredDistance.Value > blendedSquaredDistance)
                {
                    minBlendedSquaredDistance = blendedSquaredDistance;
                    blendedPoint = point;
                }
            }

            if (!targetDirection.HasValue || targetDirection.Value != blendedPoint.Value)
            {
                targetDirection = blendedPoint;

                animationSmoothingCount = 0;
            }
            else
            {
                animationSmoothingCount++;
            }

            if (animationSmoothingCount > animationSmoothingThreshold)
            {
                animator.SetFloat("BlendX", movementBehaviour.WalkingDirection.x);
                animator.SetFloat("BlendY", movementBehaviour.WalkingDirection.y);
            }

            animator.Play(animationName, 0, timePercent);

            if (timePercent >= 1f && overridingAnimationName != null)
            {
                overridingAnimationName = null;

                if (overridenSpeed.HasValue)
                    engineComponent.Entity.Speed = overridenSpeed.Value;

                if (onOverrideEnd != null) onOverrideEnd();
            }
        }
    }
}