namespace Pandora.Deck.Event {
    public class CardDiscarded: DeckEvent {
        public string Name;

        public CardDiscarded(string name) {
            Name = name;
        }
    }
}