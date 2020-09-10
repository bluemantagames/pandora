using System.Collections.Generic;

namespace Pandora.Network.Data
{
    [System.Serializable]
    public class GenericJwtPayload<T>
    {
        public long exp;
        public string jti;
        public T payload;
    }
}