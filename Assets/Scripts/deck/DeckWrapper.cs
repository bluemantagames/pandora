using System.Collections.Generic;
using UnityEngine;

namespace Pandora.Deck {
    public class DeckWrapper: ScriptableObject {
        public List<string> Cards;

        public override string ToString() {
            return string.Join(",", Cards);
        }
    }
}