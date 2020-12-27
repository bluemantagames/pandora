using Pandora.UI.Elements.Navbar;
using Pandora.UI.Menu;

namespace Pandora.UI.Menu.Event
{
    public class ViewActive : MenuEvent
    {
        public MenuView ActiveView;

        public ViewActive(MenuView view)
        {
            ActiveView = view;
        }
    }
}