using System.Collections.Generic;

namespace Pandora.Network.Data.Users
{
    public class DeckSlot
    {
        public long id { get; set; }
        public List<string> deck { get; set; }
    }
}