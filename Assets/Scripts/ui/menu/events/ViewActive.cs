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