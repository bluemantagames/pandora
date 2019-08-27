using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Engine;

namespace Pandora.Spell
{
    public class ProjectileSpellBehaviour : MonoBehaviour
    {
        public MapComponent map;
        public float speed = 3f;
        public ProjectileSpell spell;

        public GridCell Target;
        EngineComponent entityComponent;
        GridCell startCell = new GridCell(6, 0);

        void Awake() {
            entityComponent = GetComponent<EngineComponent>();
        }

        // Update is called once per frame
        void Update()
        {
            if (TeamComponent.assignedTeam == TeamComponent.topTeam) {
                transform.position = entityComponent.Entity.GetFlippedWorldPosition();
            } else {
                transform.position = entityComponent.Entity.GetWorldPosition();
            }

            if (entityComponent.Entity.GetCurrentCell() == Target) {
                spell.SpellCollided(Target);

                Destroy(this);

                gameObject.SetActive(false);

                entityComponent.Remove();
            }
        }
    }
}
