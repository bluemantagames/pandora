using System.Collections.Generic;

namespace Pandora.Network.Data
{
    [System.Serializable]
    public class PaginatedResult<T>
    {
        public long count;
        public List<T> result;
    }
}