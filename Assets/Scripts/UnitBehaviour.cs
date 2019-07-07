using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CRclone.Movement;
using CRclone.Combat;

namespace CRclone
{
    public class UnitBehaviour : MonoBehaviour {
        MovementComponent movementComponent;
        CombatBehaviour combatBehaviour;

        // Start is called before the first frame update
        void Start()
        {
            movementComponent = GetComponent<MovementComponent>();
            combatBehaviour = GetComponent<CombatBehaviour>();
            
            Debug.Log($"CombatBehaviour is {combatBehaviour}");
        }

        // Update is called once per frame
        void Update()
        {
            var state = movementComponent.Move();

            if (state.state == MovementStateEnum.EnemyApproached && !combatBehaviour.isAttacking) {
                Debug.Log("Attacking arrgh");

                combatBehaviour.AttackEnemy(state.enemy);
            } else if (state.state != MovementStateEnum.EnemyApproached && combatBehaviour.isAttacking) {
                combatBehaviour.StopAttacking();
            }

            Debug.Log("Movement state " + state);
        }
    }
}