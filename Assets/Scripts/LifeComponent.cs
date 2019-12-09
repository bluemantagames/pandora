using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pandora.Engine;
using Pandora.Combat;
using Pandora.Network;

namespace Pandora
{

    public class LifeComponent : MonoBehaviour
    {
        public int lifeValue = 100;
        Image mask;
        float maskOriginalSize;
        public int maxLife;
        public bool IsDead = false;
        public Vector2Int? LastPosition = null;

        // Start is called before the first frame update
        void Start()
        {
            mask = GetComponentInChildren<HealthbarBehaviour>().gameObject.transform.parent.gameObject.GetComponent<Image>();

            maskOriginalSize = mask.rectTransform.rect.width;
            maxLife = lifeValue;
        }

        public void Remove()
        {
            GetComponent<CombatBehaviour>().StopAttacking();

            foreach (var rigidBody in GetComponentsInChildren<Rigidbody2D>())
            {
                rigidBody.simulated = false;
            }

            foreach (var sprite in GetComponentsInChildren<SpriteRenderer>())
            {
                sprite.enabled = false;
            }

            foreach (Transform child in this.transform)
            {
                child.gameObject.SetActive(false);
            }

            var engineComponent = GetComponent<EngineComponent>();

            if (engineComponent != null)
            {
                engineComponent.Remove();
            }
        }

        private void SetLastPosition()
        {
            var sourceEntity = GetComponent<EngineComponent>().Entity;
            LastPosition = sourceEntity.GetCurrentCell().vector;
        }

        public void AssignDamage(int value)
        {
            lifeValue -= value;

            float lifePercent = (float)lifeValue / (float)maxLife;
            var idComponent = GetComponent<UnitIdComponent>();

            mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, lifePercent * maskOriginalSize);

            if (lifeValue <= 0)
            {
                IsDead = true;
                GetComponent<CombatBehaviour>().OnDead();

                var groupComponent = GetComponent<GroupComponent>();

                if (groupComponent != null)
                {
                    groupComponent.AliveObjects.Remove(gameObject);

                    if (groupComponent.AliveObjects.Count == 0)
                    {
                        CommandViewportBehaviour.Instance.RemoveCommand(groupComponent.OriginalId);
                    }
                }
                else if (idComponent != null)
                {
                    CommandViewportBehaviour.Instance.RemoveCommand(idComponent.Id);
                }

                SetLastPosition();
                Remove();

                Logger.Debug("BB I'M DYING");

                // TODO: Play "die" animation
            }
        }
    }

}