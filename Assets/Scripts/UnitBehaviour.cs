using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        public bool DebugMove = false;
        public Bounds hitbox;
        public string ComponentName
        {
            get
            {
                return "UnitBehaviour";
            }
        }

        CustomSampler moveSampler;
        Animator animator;

        public string AnimationStateName;
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
                if (!animator.GetBool("Walking"))
                {
                    animator.SetBool("Walking", true);
                    animator.speed = 0;
                }

                walkingAnimationTime += engineComponent.Entity.Speed / WalkingAnimationEngineUnits;

                if (walkingAnimationTime > 1f) {
                    walkingAnimationTime = 0f;
                }

                animator.Play(AnimationStateName, 0, walkingAnimationTime);
            }
        }
    }
}