using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Movement;
using Pandora.Combat;

namespace Pandora
{
    public class UnitBehaviour : MonoBehaviour
    {
        MovementComponent movementComponent;
        CombatBehaviour combatBehaviour;
        LifeComponent lifeComponent;
        public Bounds hitbox;

        // Start is called before the first frame update
        void Awake()
        {
            movementComponent = GetComponent<MovementComponent>();
            combatBehaviour = GetComponent<CombatBehaviour>();
            lifeComponent = GetComponent<LifeComponent>();

            Debug.Log($"CombatBehaviour is {combatBehaviour}");
        }

        // This is called from PandoraEngine every tick
        public void UnitUpdate()
        {
            if (lifeComponent.isDead) return; // Do nothing if dead

            var state = movementComponent.Move();

            if (state.state == MovementStateEnum.EnemyApproached && !combatBehaviour.isAttacking)
            {
                Debug.Log("Unit is now attacking");

                combatBehaviour.AttackEnemy(state.enemy);
            }
            else if (state.state != MovementStateEnum.EnemyApproached && combatBehaviour.isAttacking)
            {
                combatBehaviour.StopAttacking();
            }
        }
    }
}