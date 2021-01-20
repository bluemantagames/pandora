using Pandora.Events;
using Pandora.UI.Menu.Event;

namespace Pandora.UI.Menu
{
    public class MenuEventsSingleton
    {

        private static MenuEventsSingleton privateInstance = null;
        EventBus<MenuEvent> privateEventBus = new EventBus<MenuEvent>();

        public EventBus<MenuEvent> EventBus
        {
            get => privateEventBus;
        }

        private MenuEventsSingleton() { }

        public static MenuEventsSingleton instance
        {
            get
            {
                if (privateInstance == null)
                {
                    privateInstance = new MenuEventsSingleton();
                }

                return privateInstance;
            }
        }
    }
}