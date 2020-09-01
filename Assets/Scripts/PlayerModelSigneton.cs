using System.Collections.Generic;
using Pandora.Network.Data.Users;

namespace Pandora
{
    public class PlayerModelSingleton
    {
        public string Token = null;
        public User User = null;
        public List<DeckSlot> DeckSlots = null;

        private static PlayerModelSingleton privateInstance = null;

        private PlayerModelSingleton() { }

        public static PlayerModelSingleton instance
        {
            get
            {
                if (privateInstance == null)
                {
                    privateInstance = new PlayerModelSingleton();
                }

                return privateInstance;
            }
        }

        public List<string> GetActiveDeck()
        {
            if (User == null || DeckSlots == null) return null;

            var activeDeckSlot = DeckSlots.Find(deckSlot => deckSlot.id == User.activeDeckSlot);

            return activeDeckSlot.deck;
        }
    }
}