using System.Collections.Generic;
using UnityEngine.UIElements;

public class NavbarController
{
    private VisualElement navbarElement;

    private string buttonsContainerName = "NavbarButtonsContainer";
    private string genericButtonName = "NavButton";
    private string newsNavbarButtonName = "NewsNavbarButton";
    private string shopNavbarButtonName = "ShopNavbarButton";
    private string homeNavbarButtonName = "HomeNavbarButton";
    private string deckNavbarButtonName = "DeckNavbarButton";
    private string socialNavbarButtonName = "SocialNavbarButton";

    private Dictionary<string, NavbarButtonController> navControllers = new Dictionary<string, NavbarButtonController>();

    public NavbarController(VisualElement rootElement)
    {
        navbarElement = rootElement;

        var buttonsContainerElement = rootElement.Q(buttonsContainerName);

        foreach (VisualElement entry in buttonsContainerElement.Children())
        {
            var name = entry.name;
            var button = entry.Q<Button>(genericButtonName);
            var controller = new NavbarButtonController(button, () => HandleSelect(name));

            navControllers.Add(name, controller);
        }
    }

    private void HandleSelect(string currentName)
    {
        foreach (KeyValuePair<string, NavbarButtonController> entry in navControllers)
        {
            if (entry.Key != currentName) entry.Value.Deactivate();
        }
    }
}
