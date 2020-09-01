using System.Collections.Generic;

namespace Pandora.Network.Data.Users
{
    [System.Serializable]
    public class MeResponse
    {
        public User user;
        public List<DeckSlot> deckSlots;
    }
}