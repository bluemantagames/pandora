using Pandora.Events;
using Pandora.UI.Menu.Event;

namespace Pandora.UI.Menu
{
    public class MenuEventsSingleton
    {

        private static MenuEventsSingleton _instance = null;
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
                if (_instance == null)
                {
                    _instance = new MenuEventsSingleton();
                }

                return _instance;
            }
        }

        public static void Reset()
        {
            _instance = null;
        }
    }
}