using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pandora.Engine;
using Pandora.Combat;

namespace Pandora
{

    public class LifeComponent : MonoBehaviour
    {
        public int lifeValue = 100;
        public Image mask;
        float maskOriginalSize;
        public int maxLife;
        public bool isDead = false;

        // Start is called before the first frame update
        void Start()
        {
            maskOriginalSize = mask.rectTransform.rect.width;
            maxLife = lifeValue;
        }

        public void Remove() {
            GetComponent<CombatBehaviour>().StopAttacking();

            foreach (var rigidBody in GetComponentsInChildren<Rigidbody2D>())
            {
                rigidBody.simulated = false;
            }

            foreach (var sprite in GetComponentsInChildren<SpriteRenderer>())
            {
                sprite.enabled = false;
            }
            
            foreach (Transform child in this.transform) {
                child.gameObject.SetActive(false);
            }

            var engineComponent = GetComponent<EngineComponent>();

            if (engineComponent != null)
            {
                engineComponent.Remove();
            }
        }

        public void AssignDamage(int value)
        {
            lifeValue -= value;

            float lifePercent = (float) lifeValue / (float) maxLife;

            mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, lifePercent * maskOriginalSize);


            if (lifeValue <= 0)
            {
                isDead = true;
                GetComponent<CombatBehaviour>().OnDead();

                Remove();

                Debug.Log("BB I'M DYING");

                // TODO: Play "die" animation
            }
        }
    }

}