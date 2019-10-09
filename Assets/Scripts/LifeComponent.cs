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

        public void AssignDamage(int value)
        {
            lifeValue -= value;

            float lifePercent = lifeValue / maxLife;

            mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, lifePercent * maskOriginalSize);


            if (lifeValue <= 0)
            {
                isDead = true;

                Debug.Log("BB I'M DYING");

                GetComponent<CombatBehaviour>().StopAttacking();
                GetComponent<CombatBehaviour>().OnDead();

                foreach (var rigidBody in GetComponentsInChildren<Rigidbody2D>())
                {
                    rigidBody.simulated = false;
                }

                foreach (var renderer in GetComponentsInChildren<SpriteRenderer>())
                {
                    Debug.Log("Disabling renderer");

                    renderer.enabled = false;
                }

                var engineComponent = GetComponent<EngineComponent>();

                if (engineComponent == null)
                {
                    Debug.LogWarning("Could not find engine component");
                }

                engineComponent?.Remove();

                // TODO: Play "die" animation
            }
        }
    }

}