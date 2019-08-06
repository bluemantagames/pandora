using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CRclone.Spell
{
    public class ProjectileSpellBehaviour : MonoBehaviour
    {
        public MapComponent map;
        public float speed = 3f;
        public ProjectileSpell spell;

        Vector2 spawnPosition;
        GridCell startCell = new GridCell(6, 0);
        Vector2 direction;

        void Awake()
        {
            spawnPosition = transform.position;

            transform.position = map.GridCellToWorldPosition(startCell);

            Vector2 position = transform.position;

            direction = (spawnPosition - position).normalized;
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 position = direction * speed * Time.deltaTime;
            Vector3 spawnPosition = this.spawnPosition;

            transform.position += position;

            var orderedCurrentX = transform.position.x / direction.x;
            var orderedTargetX = spawnPosition.x / direction.x;

            if (orderedCurrentX >= orderedTargetX) {
                spell.SpellCollided(map.WorldPositionToGridCell(spawnPosition));

                Destroy(this);

                gameObject.SetActive(false);
            }
        }
    }
}
