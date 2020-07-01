using System.Collections.Generic;

namespace Pandora.Network.Data.Matchmaking
{
    public class UserMatchTokenPayload
    {
        public long userId;
        public List<string> deck;
        public string matchToken;
    }
}