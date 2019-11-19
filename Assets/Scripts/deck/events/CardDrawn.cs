namespace Pandora.Deck.Event {
    public class CardDrawn: DeckEvent {
        public string Name;

        public CardDrawn(string name) {
            Name = name;
        }
    }
}