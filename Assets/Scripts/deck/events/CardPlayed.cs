namespace Pandora.Deck.Event {
    public class CardPlayed: DeckEvent {
        public string Name;

        public CardPlayed(string name) {
            Name = name;
        }
    }
}