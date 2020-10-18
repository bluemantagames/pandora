using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Pandora
{
    public class CommandViewportBehaviour : MonoBehaviour
    {
        List<GameObject> handlers = new List<GameObject> { };

        public GameObject CommandImage;

        static CommandViewportBehaviour _instance;

        public static CommandViewportBehaviour Instance
        {
            get => _instance;
        }

        RectTransform rect;

        public void RemoveCommand(string id)
        {
            for (var i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];

                if (handler == null) continue;

                if (handler.GetComponent<CommandImageBehaviour>().UnitId == id)
                {
                    handlers[i] = null;

                    Destroy(handler);

                    break;
                }
            }
        }


        public void AddCommand(string unit, string id)
        {
            var cards = Resources.LoadAll("Cards/", typeof(GameObject));

            GameObject foundCard = null;

            foreach (var cardObject in cards)
            {
                var cardGameObject = cardObject as GameObject;

                if (cardGameObject.GetComponent<CardBehaviour>().UnitName == unit)
                {
                    foundCard = cardGameObject;

                    break;
                }
            }

            if (foundCard == null) return;

            var card = foundCard.GetComponent<Image>();

            var handler = Instantiate(CommandImage, transform.position, Quaternion.identity, transform);

            var added = false;

            // fill an empty position if there is one
            for (var i = 0; i < handlers.Count; i++)
            {
                if (handlers[i] == null)
                {
                    handlers[i] = handler;

                    added = true;

                    break;
                }
            }

            // add the command at the end otherwise
            if (!added)
            {
                handlers.Add(handler);
            }

            handler.GetComponent<Image>().sprite = card.sprite;
            handler.GetComponent<CommandImageBehaviour>().UnitId = id;

            var handlerRectTransform = handler.GetComponent<RectTransform>();
            
            var heightFactor = handlerRectTransform.rect.height / rect.rect.height;

            handler.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                handlerRectTransform.rect.width / heightFactor
            );

            handler.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                rect.rect.height
            );
        }

        void Awake()
        {
            _instance = this;
        }

        void Start() {
            rect = GetComponent<RectTransform>();
        }

        void Update()
        {

        }
    }

}