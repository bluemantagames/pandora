using UnityEngine;

namespace Pandora.Deck {
    public class HandCard {
        public HandCard(string name, GameObject card) {
            Name = name;
            CardObject = card;
        }

        public string Name;
        public GameObject CardObject;
    }
}