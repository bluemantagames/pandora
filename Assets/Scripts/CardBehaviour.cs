using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Pandora.Movement;
using Pandora.Spell;
using Pandora.Network;
using Pandora.Deck;
using Pandora.Deck.UI;

namespace Pandora
{
    public class CardBehaviour : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerClickHandler
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
        public bool FixedInGame = false;
        public bool Global = false;
        public bool MulliganSelected = false;

        Image imageComponent;
        GraphicRaycaster raycasterComponent;
        Color defaultColor;
        bool disabled = false;

        public bool IsUI
        {
            get => _isUI;

            set
            {
                _isUI = value;

                // Disable mana image on UI
                // and swap this component for MenuCardBehaviour
                if (value)
                {
                    var menuCardBehaviour = gameObject.AddComponent<MenuCardBehaviour>();

                    menuCardBehaviour.Canvas = GameObject.Find("Canvas");
                    menuCardBehaviour.CardName = CardName;

                    menuCardBehaviour.Load();

                    disabled = true;

                    foreach (Transform child in transform)
                    {
                        Destroy(child.gameObject);
                    }

                    Destroy(this);
                }
            }
        }

        bool canBeSpawned = false;
        bool _isUI = false;

        private void CleanUpDrag(bool returnToPosition)
        {
            if (disabled) return;

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
            if (disabled) return;

            if (!originalPosition.HasValue)
            {
                originalPosition = transform.position;
            }

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

                canBeSpawned = map.OnUICardCollision(Puppet, IsAquatic, Global, map.LoadCard(UnitName));

                GetComponent<Image>().enabled = false;
            }
            else
            {
                CleanUpDrag(false);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (disabled) return;

            var movement = Card.GetComponent<MovementBehaviour>();
            var projectileSpell = Card.GetComponent<ProjectileSpellBehaviour>();

            if (map != null && canBeSpawned)
            {
                if (movement != null) movement.map = map;
                if (projectileSpell != null) projectileSpell.map = map;

                map.SpawnCard(UnitName, Team, RequiredMana);

                LocalDeck.Instance.PlayCard(new Card(CardName));

                map.DestroyPuppet();

                if (!FixedInGame)
                {
                    GetComponent<Image>().enabled = false;

                    SetChildrenActive(false);

                    Destroy(this);
                } else {
                    CleanUpDrag(true);
                }

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


        void OnApplicationQuit()
        {
            if (disabled) return;

            NetworkControllerSingleton.instance.Stop();
        }

        void SetChildrenActive(bool active)
        {
            if (disabled) return;

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

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[MULLIGAN] Clicked {CardName}");
            LocalDeck.Instance.CardSelect(new Card(CardName));
        }

        void Awake()
        {
            raycasterComponent = gameObject.GetComponent<GraphicRaycaster>();

            imageComponent = gameObject.GetComponent<Image>();
            defaultColor = imageComponent.color;
        }

        void Update() 
        {

            imageComponent.color = (MulliganSelected == true) ? Color.yellow : defaultColor;
        }
    }
}