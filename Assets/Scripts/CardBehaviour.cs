using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CRclone.Movement;
using CRclone.Spell;

namespace CRclone
{
    public class CardBehaviour : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        Vector3 originalPosition;
        MapListener mapListener;

        public GameObject puppet;
        public GameObject card;
        public int team = 1;

        private void CleanUpDrag(bool returnToPosition)
        {
            if (mapListener != null)
            {
                mapListener.DestroyPuppet();
                mapListener = null;
            }

            GetComponent<Image>().enabled = true;

            if (returnToPosition)
                transform.position = originalPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition;

            var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(transform.position), Vector2.up, 0f, LayerMask.GetMask("Map"));

            if (hit.collider != null)
            {
                mapListener = hit.collider.gameObject.GetComponent<MapListener>();

                mapListener.OnUICardCollision(puppet);

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

            if (mapListener != null)
            {
                if (movement != null) movement.map = mapListener;
                if (projectileSpell != null) projectileSpell.map = mapListener;

                mapListener.SpawnCard(card, team);
            }



            CleanUpDrag(true);
        }


        // Start is called before the first frame update
        void Awake()
        {
            if (originalPosition == null)
                originalPosition = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}