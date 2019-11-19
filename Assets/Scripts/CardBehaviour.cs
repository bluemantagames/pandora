using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Pandora.Movement;
using Pandora.Spell;
using Pandora.Network;
using Pandora.Deck;

namespace Pandora
{
    public class CardBehaviour : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        Vector3? originalPosition = null;
        MapComponent map;

        public GameObject Puppet;
        public GameObject Card;
        public int Team = 1;
        public string UnitName;
        public string CardName;
        public int RequiredMana = 0;
        public bool IsAquatic = false;

        bool canBeSpawned = false;

        private void CleanUpDrag(bool returnToPosition)
        {
            if (map != null)
            {
                map.DestroyPuppet();
                map = null;
            }

            GetComponent<Image>().enabled = true;

            if (returnToPosition)
            {
                transform.position = originalPosition.Value;

                SetChildrenActive(true);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition;

            SetChildrenActive(false);

            var hit = Physics2D.Raycast(
                Camera.main.ScreenToWorldPoint(transform.position), Vector2.up, 0f,
                LayerMask.GetMask("Map")
            );

            if (hit.collider != null && hit.collider.gameObject.GetComponent<MapComponent>() != null)
            {
                Debug.Log("Calling OnUICardCollision");

                map = hit.collider.gameObject.GetComponent<MapComponent>();

                canBeSpawned = map.OnUICardCollision(Puppet, IsAquatic);

                GetComponent<Image>().enabled = false;
            }
            else
            {
                CleanUpDrag(false);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var movement = Card.GetComponent<MovementBehaviour>();
            var projectileSpell = Card.GetComponent<ProjectileSpellBehaviour>();

            if (map != null && canBeSpawned)
            {
                if (movement != null) movement.map = map;
                if (projectileSpell != null) projectileSpell.map = map;

                map.SpawnCard(UnitName, Team, RequiredMana);

                LocalDeck.Instance.PlayCard(new Card(CardName));

                map.DestroyPuppet();

                GetComponent<Image>().enabled = false;

                SetChildrenActive(false);

                Destroy(this);
            }
            else
            {
                CleanUpDrag(true);
            }

        }

        void Start()
        {
            GetComponentInChildren<Text>().text = (RequiredMana / 10).ToString();
        }


        void Update()
        {
            if (!originalPosition.HasValue)
            {
                originalPosition = transform.position;
            }
        }

        void OnApplicationQuit()
        {
            NetworkControllerSingleton.instance.Stop();
        }

        void SetChildrenActive(bool active)
        {
            foreach (Transform child in transform)
            {
                var image = child.GetComponent<Image>();

                if (image != null)
                {
                    image.enabled = active;
                }

                child.gameObject.SetActive(active);
            }
        }
    }
}