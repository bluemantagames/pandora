using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora;

namespace Pandora.Combat
{
    public class OrderCombatVFXFixer : MonoBehaviour, CombatVFXFixer
    {
        public int TopDirectionOrder = 1;
        public int BottomDirectionOrder = 1;
        public int CenterDirectionOrder = 1;

        ParticleSystemRenderer particleSystemRenderer;

        void Awake()
        {
            particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
        }

        public void FixVFX(Vector2Int enemyDirection, Vector2 rawDirection)
        {
            if (particleSystemRenderer == null) return;

            if (enemyDirection.y > 0)
                particleSystemRenderer.sortingOrder = TopDirectionOrder;
            else if (enemyDirection.y < 0)
                particleSystemRenderer.sortingOrder = BottomDirectionOrder;
            else
                particleSystemRenderer.sortingOrder = CenterDirectionOrder;
        }
    }
}