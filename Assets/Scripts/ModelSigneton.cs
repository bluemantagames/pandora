using System.Collections.Generic;
using Pandora.Network.Data.Users;

namespace Pandora
{
    public class ModelSingleton
    {
        public string Token = null;
        public User User = null;
        public List<DeckSlot> DeckSlots = null;

        private static ModelSingleton privateInstance = null;

        private ModelSingleton() { }

        public static ModelSingleton instance
        {
            get
            {
                if (privateInstance == null)
                {
                    privateInstance = new ModelSingleton();
                }

                return privateInstance;
            }
        }
    }
}