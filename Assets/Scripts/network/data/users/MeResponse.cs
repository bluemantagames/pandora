using System.Collections.Generic;

namespace Pandora.Network.Data.Users
{
    public class MeResponse
    {
        public User user { get; set; }
        public List<DeckSlot> deckSlots { get; set; }
    }
}