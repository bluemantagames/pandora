namespace Pandora.Deck.Event {
    public class CardSelected: DeckEvent {
        public string Name;

        public CardSelected(string name) {
            Name = name;
        }
    }
}