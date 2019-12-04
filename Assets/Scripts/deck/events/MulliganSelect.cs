namespace Pandora.Deck.Event {
    public class MulliganSelect: DeckEvent {
        public string Name;

        public MulliganSelect(string name) {
            Name = name;
        }
    }
}