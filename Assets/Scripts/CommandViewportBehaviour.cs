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

        public void DrawCommands()
        {
            var rect = GetComponent<RectTransform>();

            for (var i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];

                if (handler == null) continue;

                var handlerRect = handler.GetComponent<RectTransform>();
                var width = handlerRect.rect.width;
                var height = handlerRect.rect.height;
                var handlerPosition = new Vector2((i * width) + width / 2, -rect.rect.height / 2);

                handlerRect.localPosition = handlerPosition;
            }
        }

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

            DrawCommands();
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
                }
            }

            // add the command at the end otherwise
            if (!added)
            {
                handlers.Add(handler);
            }

            handler.GetComponent<Image>().sprite = card.sprite;
            handler.GetComponent<CommandImageBehaviour>().UnitId = id;

            DrawCommands();
        }

        void Awake()
        {
            _instance = this;
        }

        void Update()
        {

        }
    }

}