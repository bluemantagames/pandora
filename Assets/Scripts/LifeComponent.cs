using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CRclone.Combat;

namespace CRclone
{

    public class LifeComponent : MonoBehaviour
    {
        public float lifeValue = 100;
        public Image mask;
        float maskOriginalSize;
        float maxLife;
        public bool isDead = false;

        // Start is called before the first frame update
        void Start()
        {
            maskOriginalSize = mask.rectTransform.rect.width;
            maxLife = lifeValue;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AssignDamage(float value)
        {
            lifeValue -= value;

            float lifePercent = lifeValue / maxLife;

            mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, lifePercent * maskOriginalSize);

            if (lifeValue <= 0)
            {
                isDead = true;

                Debug.Log("BB I'M DYING");

                GetComponent<Rigidbody2D>().simulated = false;
                GetComponent<CombatBehaviour>().StopAttacking();
                GetComponent<SpriteRenderer>().enabled = false;

                // TODO: Play "die" animation
            }
        }
    }

}