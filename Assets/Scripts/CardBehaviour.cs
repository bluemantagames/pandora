﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Pandora.Movement;
using Pandora.Spell;
using Pandora.Network;

namespace Pandora
{
    public class CardBehaviour : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        Vector3? originalPosition = null;
        MapComponent map;

        public GameObject puppet;
        public GameObject card;
        public int team = 1;
        public string cardName;

        private void CleanUpDrag(bool returnToPosition)
        {
            if (map != null)
            {
                map.DestroyPuppet();
                map = null;
            }

            GetComponent<Image>().enabled = true;

            if (returnToPosition)
                transform.position = originalPosition.Value;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition;

            var hit = Physics2D.Raycast(
                Camera.main.ScreenToWorldPoint(transform.position), Vector2.up, 0f,
                LayerMask.GetMask("Map")
            );

            if (hit.collider != null && hit.collider.gameObject.GetComponent<MapComponent>() != null)
            {
                Debug.Log("Calling OnUICardCollision");

                map = hit.collider.gameObject.GetComponent<MapComponent>();

                map.OnUICardCollision(puppet);

                GetComponent<Image>().enabled = false;
            }
            else
            {
                CleanUpDrag(false);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var movement = card.GetComponent<MovementComponent>();
            var projectileSpell = card.GetComponent<ProjectileSpellBehaviour>();

            if (map != null)
            {
                if (movement != null) movement.map = map;
                if (projectileSpell != null) projectileSpell.map = map;

                map.SpawnCard(cardName, team);
            }

            CleanUpDrag(true);
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
    }
}