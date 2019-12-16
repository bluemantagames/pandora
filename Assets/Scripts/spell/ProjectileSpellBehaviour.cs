using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.Engine;

namespace Pandora.Spell
{
    public class ProjectileSpellBehaviour : MonoBehaviour, EngineBehaviour
    {
        public MapComponent map;
        public int Speed = 1200;
        public ProjectileSpell spell;

        public GridCell Target;
        EngineComponent entityComponent;
        public GridCell StartCell = new GridCell(6, 0);

        public string ComponentName {
            get => "ProjectileSpellBehaviour";
        }

        void Awake()
        {
            entityComponent = GetComponent<EngineComponent>();
        }

        public void TickUpdate(uint msElapsed)
        {
            if (entityComponent.Entity.GetCurrentCell() == Target)
            {
                spell.SpellCollided(Target);

                Destroy(this);

                gameObject.SetActive(false);

                entityComponent.Remove();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (TeamComponent.assignedTeam == TeamComponent.topTeam)
            {
                transform.position = entityComponent.Entity.GetFlippedWorldPosition();
            }
            else
            {
                transform.position = entityComponent.Entity.GetWorldPosition();
            }
        }
    }
}
