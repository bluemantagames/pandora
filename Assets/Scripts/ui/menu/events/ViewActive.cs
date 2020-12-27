using Pandora.UI.Elements.Navbar;

namespace Pandora.UI.Menu.Event
{
    public class ViewActive : MenuEvent
    {
        public NavbarButton ActiveView;

        public ViewActive(NavbarButton view)
        {
            ActiveView = view;
        }
    }
}