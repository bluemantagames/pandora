using System.Collections.Generic;

namespace Pandora.Network.Data.Users
{
    [System.Serializable]
    public class DeckSlot
    {
        public long id;
        public List<string> deck;
    }
}