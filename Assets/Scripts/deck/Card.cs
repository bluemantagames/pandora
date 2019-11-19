using UnityEngine.JSONSerializeModule;

namespace Pandora.Deck {
    [Serializable]
    public class Card {
        public Card(string name) {
            Name = name;
        }

        public string Name;
    }
}