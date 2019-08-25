using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Pandora;
using Pandora.Engine;
using Pandora.Movement;

namespace Pandora.Combat
{
    public class SimpleProjectileBehaviour : MonoBehaviour, ProjectileBehaviour, CollisionCallback
    {
        Rigidbody2D body;

        public GameObject parent { get; set; }
        public float speed = 1f;
        public Enemy target { get; set; }
        public MapComponent map { private get; set; }
        private EngineEntity engineEntity;

        public void Collided(EngineEntity other)
        {
            Debug.Log("Collided with " + other.GameObject);

            if (other.GameObject == target.enemy)
            {
                Debug.Log("Collided with target " + other);

                var behaviour = parent.GetComponent<CombatBehaviour>();

                if (behaviour != null)
                {
                    behaviour.ProjectileCollided();
                }
                else
                {
                    Debug.LogWarning("Could not find ProjectileCollided in parent");
                }

                gameObject.SetActive(false);
                map.engine.RemoveEntity(engineEntity);
                Destroy(this);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            engineEntity = map.engine.AddEntity(gameObject, speed, map.WorldPositionToGridCell(transform.position), false, null);

            engineEntity.CollisionCallback = this;

            body = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            // direction from us to the target
            var direction = (target.enemy.transform.position - transform.position).normalized;
            var angle = Vector2.SignedAngle(Vector2.up, direction);

            var shouldBeFlipped = TeamComponent.assignedTeam == TeamComponent.topTeam;

            Debug.Log($"Angling projectiles at {angle}");

            // rotate the projectile towards the target
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // Move the projectile forward
            transform.position = shouldBeFlipped ? engineEntity.GetFlippedWorldPosition() : engineEntity.GetWorldPosition();

            EngineEntity targetEntity;

            var towerComponent = target.enemy.GetComponent<TowerPositionComponent>();

            if (towerComponent != null)
            {
                targetEntity = towerComponent.TowerEntity;
            }
            else
            {
                targetEntity = target.enemy.GetComponent<MovementComponent>().engineEntity;
            }


            engineEntity.SetTarget(targetEntity);
        }
    }
}