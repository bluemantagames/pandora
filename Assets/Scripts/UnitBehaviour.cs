using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Profiling;
using Pandora.Movement;
using Pandora.Combat;
using Pandora.Engine;
using System;

namespace Pandora
{
    public class UnitBehaviour : MonoBehaviour, EngineBehaviour
    {
        MovementBehaviour movementBehaviour;
        CombatBehaviour combatBehaviour;
        LifeComponent lifeComponent;
        EngineComponent engineComponent;
        TeamComponent teamComponent;
        public bool DebugMove = false;

        // represents whether we are handling movement / animation or another component is doing it
        bool areWeHandling = true;
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

        // Start is called before the first frame update
        void Awake()
        {
            movementBehaviour = GetComponent<MovementBehaviour>();
            combatBehaviour = GetComponent<CombatBehaviour>();
            lifeComponent = GetComponent<LifeComponent>();

            moveSampler = CustomSampler.Create($"Move() {gameObject.name}");
            animator = GetComponent<Animator>();
            engineComponent = GetComponent<EngineComponent>();
            teamComponent = GetComponent<TeamComponent>();

            if (BlueController || RedController)
            {
                animator.runtimeAnimatorController =
                    (teamComponent.Team == TeamComponent.assignedTeam) ? BlueController : RedController;
            }


            Logger.Debug($"CombatBehaviour is {combatBehaviour}");
        }

        // This is called from PandoraEngine every tick
        public void TickUpdate(uint timeLapsed)
        {
            if (lifeComponent.IsDead || !areWeHandling) return; // Do nothing if dead

            if (overridingAnimationName != null)
            {
                overridingTotalTimePassed += timeLapsed;

                playAnimation((float) overridingTotalTimePassed / (float) overridingAnimationMsLength, overridingAnimationName);

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
                var timePercent = engineComponent.Entity.Speed / ((float)WalkingAnimationEngineUnits);

                walkingAnimationTime += timePercent;

                if (walkingAnimationTime > 1f)
                {
                    walkingAnimationTime = 0f;
                }

                playAnimation(timePercent, overridingAnimationName);
            }
        }

        /// <summary>Other behaviours can use this method to stop our handling of animation and movement</summary>
        public void PauseBehaviour()
        {
            areWeHandling = false;
        }

        /// <summary>Other behaviours can use this method to resume our handling after pausing it with PauseBehaviour</summary>
        public void UnpauseBehaviour()
        {
            areWeHandling = true;
        }

        public void PlayAnimation(string animationStateName, uint animationMsLength, Action animationEndCallback)
        {
            overridingAnimationName = animationStateName;
            overridingAnimationMsLength = animationMsLength;
            onOverrideEnd = animationEndCallback;
        }

        void playAnimation(float timePercent, string animationName)
        {
            animator.speed = 0;

            animator.SetFloat("BlendX", movementBehaviour.WalkingDirection.x);
            animator.SetFloat("BlendY", movementBehaviour.WalkingDirection.y);

            animator.Play(WalkingAnimationStateName, 0, timePercent);

            if (timePercent >= 1f)
            {
                overridingAnimationName = null;

                if (onOverrideEnd != null) onOverrideEnd();
            }
        }
    }
}