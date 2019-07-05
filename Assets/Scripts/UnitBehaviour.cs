using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CRclone.Movement;

namespace CRclone
{
    public class UnitBehaviour : MonoBehaviour {
        MovementComponent movementComponent;
        MeleeCombatBehaviour meleeCombatBehaviour;

        // Start is called before the first frame update
        void Start()
        {
            movementComponent = GetComponent<MovementComponent>();
            meleeCombatBehaviour = GetComponent<MeleeCombatBehaviour>();
        }

        // Update is called once per frame
        void Update()
        {
            var state = movementComponent.Move();

            if (state.state == MovementStateEnum.EnemyApproached && !meleeCombatBehaviour.isAttacking) {
                meleeCombatBehaviour.AttackEnemy(state.enemy);
            }

            Debug.Log("Movement state " + state);
        }
    }
}