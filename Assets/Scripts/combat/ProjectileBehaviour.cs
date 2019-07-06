using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CRclone.Combat
{
    public class ProjectileBehaviour : MonoBehaviour
    {
        Rigidbody2D body;

        public float speed = 1f;
        public Enemy target;

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("Collided with " + other);

            if (other.gameObject == target.enemy)
            {
                Debug.Log("Collided with target " + other);

                foreach (var rangedCombat in GetComponentsInParent<RangedCombatBehaviour>())
                {
                    rangedCombat.ProjectileCollided();

                    Destroy(this);

                    gameObject.SetActive(false);
                }
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

            Debug.Log("Angling projectiles at" + angle);

            // rotate the projectile towards the target
            body.SetRotation(angle);

            // Move the projectile forward
            body.MovePosition(transform.position + direction * (Time.deltaTime * speed));
        }
    }
}