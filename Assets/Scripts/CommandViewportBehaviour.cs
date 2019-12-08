using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pandora
{
    public class CommandViewportBehaviour : MonoBehaviour
    {
        List<GameObject> handlers = new List<GameObject> {};

        public GameObject CommandImage;

        static CommandViewportBehaviour _instance;

        public static CommandViewportBehaviour Instance {
            get => _instance;
        }

        public void DrawCommands() {
            var rect = GetComponent<RectTransform>();

            for (var i = 0; i < handlers.Count; i++) {
                var handler = handlers[i];

                var handlerRect = handler.GetComponent<RectTransform>();
                var width = handlerRect.rect.width;
                var height = handlerRect.rect.height;
                var handlerPosition = new Vector2((i * width) + width / 2, -rect.rect.height / 2);

                handlerRect.localPosition = handlerPosition;
            }
        }

        public void RemoveCommand(string id) {
            foreach (var handler in handlers) {
                if (handler.GetComponent<CommandImageBehaviour>().UnitId == id) {
                    handlers.Remove(handler);
                    Destroy(handler);

                    break;
                }
            }

            DrawCommands();
        }


        public void AddCommand(string unit, string id) {
            var cards = Resources.LoadAll("Cards/", typeof(GameObject));

            GameObject foundCard = null;

            foreach (var cardObject in cards) {
                var cardGameObject = cardObject as GameObject;

                if (cardGameObject.GetComponent<CardBehaviour>().UnitName == unit) {
                    foundCard = cardGameObject;

                    break;
                }
            }

            var card = foundCard.GetComponent<Image>();

            var handler = Instantiate(CommandImage, transform.position, Quaternion.identity, transform);

            handlers.Add(handler);

            handler.GetComponent<Image>().sprite = card.sprite;
            handler.GetComponent<CommandImageBehaviour>().UnitId = id;

            DrawCommands();
        }

        void Start()
        {
            _instance = this;
        }

        void Update()
        {

        }
    }

}