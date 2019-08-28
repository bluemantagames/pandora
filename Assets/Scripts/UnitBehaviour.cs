﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Movement;
using Pandora.Combat;
using Pandora.Engine;

namespace Pandora
{
    public class UnitBehaviour : MonoBehaviour, EngineBehaviour
    {
        MovementComponent movementComponent;
        CombatBehaviour combatBehaviour;
        LifeComponent lifeComponent;
        public Bounds hitbox;
        public string ComponentName {
            get {
                return "UnitBehaviour";
            }
        } 

        // Start is called before the first frame update
        void Awake()
        {
            movementComponent = GetComponent<MovementComponent>();
            combatBehaviour = GetComponent<CombatBehaviour>();
            lifeComponent = GetComponent<LifeComponent>();

            Debug.Log($"CombatBehaviour is {combatBehaviour}");
        }

        // This is called from PandoraEngine every tick
        public void TickUpdate(uint timeLapsed)
        {
            if (lifeComponent.isDead) return; // Do nothing if dead

            var state = movementComponent.Move();

            if (state.state == MovementStateEnum.EnemyApproached)
            {
                combatBehaviour.AttackEnemy(state.enemy, timeLapsed);
            }
            else if (state.state != MovementStateEnum.EnemyApproached && combatBehaviour.isAttacking)
            {
                combatBehaviour.StopAttacking();
            }
        }
    }
}