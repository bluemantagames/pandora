using System.Collections.Generic;
using Pandora.Network.Data.Users;

namespace Pandora
{
    public class PlayerModelSingleton
    {
        public string Token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjE2MjI4MjY5ODcsImp0aSI6IjEyZTA2NDY3MDVlMTE2ZGFjMDQzZTBiN2UwZjZmZjMwIiwicGF5bG9hZCI6eyJ1c2VySWQiOjF9fQ.VD-NjetNKaH7WIagtEfgu3TpyE86V1nL-yYX2ybJLGg";
        public User User = null;
        public List<DeckSlot> DeckSlots = null;
        public int leaderboardPosition;

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