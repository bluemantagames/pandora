using UnityEngine;
using Pandora.Combat;

namespace Pandora.Command
{
    public class RangerCommand : MonoBehaviour, CommandBehaviour
    {
        public Color EnragedColor = Color.red;
        Color originalColor;
        public int AttackMultiplier = 7;

        SpriteRenderer rangerRenderer;
        MeleeCombatBehaviour combatBehaviour;

        public void InvokeCommand()
        {
            combatBehaviour.NextAttackMultiplier = AttackMultiplier;
        }

        void Awake()
        {
            combatBehaviour = GetComponentInParent<MeleeCombatBehaviour>();
            rangerRenderer = GetComponentInParent<SpriteRenderer>();
            originalColor = rangerRenderer.color;
        }

        void Update()
        {
            if (rangerRenderer.color != EnragedColor && combatBehaviour.NextAttackMultiplier.HasValue)
            {
                rangerRenderer.color = EnragedColor;
            }

            if (rangerRenderer.color == EnragedColor && !combatBehaviour.NextAttackMultiplier.HasValue)
            {
                rangerRenderer.color = originalColor;
            }
        }

    }
}