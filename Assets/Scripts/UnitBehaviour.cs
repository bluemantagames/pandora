using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Profiling;
using Pandora.Movement;
using Pandora.Combat;
using Pandora.Engine;

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
            if (lifeComponent.IsDead) return; // Do nothing if dead

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
                animator.SetBool("Walking", false);

                combatBehaviour.AttackEnemy(state.enemy, timeLapsed);
            }
            else if (state.state != MovementStateEnum.EnemyApproached && combatBehaviour.isAttacking)
            {
                combatBehaviour.StopAttacking();

                walkingAnimationTime = 0;
            }
            else if (WalkingAnimationEnabled)
            {
                animator.speed = 0;

                var timePercent = engineComponent.Entity.Speed / ((float)WalkingAnimationEngineUnits);

                walkingAnimationTime += timePercent;

                if (walkingAnimationTime > 1f)
                {
                    walkingAnimationTime = 0f;
                }

                animator.SetFloat("BlendX", movementBehaviour.WalkingDirection.x);
                animator.SetFloat("BlendY", movementBehaviour.WalkingDirection.y);

                animator.Play(WalkingAnimationStateName, 0, walkingAnimationTime);
            }
        }
    }
}