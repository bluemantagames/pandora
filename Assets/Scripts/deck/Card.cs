
namespace Pandora.Deck {
    public class Card {
        public Card(string name) {
            Name = name;
        }

        public string Name;

        public override string ToString()
        {
            return $"Card: {Name}";
        }
    }
}