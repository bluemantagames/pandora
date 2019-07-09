using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CRclone;

namespace CRclone.Combat
{
    public class SimpleProjectileBehaviour : MonoBehaviour, ProjectileBehaviour
    {
        Rigidbody2D body;

        public GameObject parent { get; set; }
        public float speed = 1f;
        public Enemy target { get; set; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("Collided with " + other);

            if (other.gameObject == target.enemy)
            {
                Debug.Log("Collided with target " + other);

                var behaviour = parent.GetComponent<RangedCombatBehaviour>();

                if (behaviour != null) {
                    behaviour.ProjectileCollided();
                } else {
                    Debug.LogWarning("Could not find ProjectileCollided in parent");
                }

                Destroy(this);

                gameObject.SetActive(false);
            }
        }

        // Start is called before the first frame update
        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            // direction from us to the target
            var direction = (target.enemy.transform.position - transform.position).normalized;
            var angle = Vector2.SignedAngle(Vector2.up, direction);

            Debug.Log($"Angling projectiles at {angle}");

            // rotate the projectile towards the target
            body.SetRotation(angle);

            // Move the projectile forward
            body.MovePosition(transform.position + direction * (Time.deltaTime * speed));
        }
    }
}