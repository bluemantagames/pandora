using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
        public int speed = 1800;

        public int Speed {
            get => speed;
        }

        public Enemy target { get; set; }
        public MapComponent map { private get; set; }
        private EngineEntity engineEntity;
        public int StartRotationDegrees = 0;
        public bool IsAoe = false;
        public int EngineUnitsRadius = 0;

        public void Collided(EngineEntity other, uint passed)
        {
            if (other.GameObject == target.enemy)
            {
                var behaviour = parent.GetComponent<CombatBehaviour>();

                if (behaviour != null)
                {
                    behaviour.ProjectileCollided(target);
                }
                else
                {
                    Debug.LogWarning("Could not find ProjectileCollided in parent");
                }


                if (IsAoe)
                {
                    var hitbox = engineEntity.Engine.GetEntityBounds(engineEntity);

                    var maxDimension = Math.Max(
                        hitbox.UpperRight.x - hitbox.LowerLeft.x,
                        hitbox.UpperRight.y - hitbox.LowerLeft.y
                    );

                    var hitEntities = engineEntity.Engine.FindInRadius(hitbox.Center, EngineUnitsRadius + maxDimension, true);

                    Debug.Log($"Searching in {EngineUnitsRadius + maxDimension}");

                    foreach (var entity in hitEntities)
                    {
                        if (entity.GameObject == target.enemy || entity.GameObject == gameObject || !entity.IsRigid) continue;

                        Debug.Log($"Hit {entity}");

                        behaviour.ProjectileCollided(new Enemy(entity.GameObject));
                    }
                }

                gameObject.SetActive(false);
                map.engine.RemoveEntity(engineEntity);

                Destroy(this);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            body = GetComponent<Rigidbody2D>();

            engineEntity = GetComponent<EngineComponent>().Entity;
        }

        // Update is called once per frame
        void Update()
        {
            // direction from us to the target
            var direction = (target.enemy.transform.position - transform.position).normalized;
            var angle = Vector2.SignedAngle(Vector2.up, direction) + StartRotationDegrees;

            var shouldBeFlipped = TeamComponent.assignedTeam == TeamComponent.topTeam;

            Debug.Log($"Angling projectiles at {angle}");

            // rotate the projectile towards the target
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // Move the projectile forward
            transform.position = shouldBeFlipped ? engineEntity.GetFlippedWorldPosition() : engineEntity.GetWorldPosition();

            // TODO: Play "miss" animation, and then remove the entity
            if (target.enemyEntity.GameObject.GetComponent<LifeComponent>().IsDead)
            {
                gameObject.SetActive(false);

                Destroy(this);
            }
        }
    }
}