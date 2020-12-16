using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

namespace Pandora.UI.Elements.Navbar
{
    public enum NavbarButton
    {
        NewsNavbarButton,
        ShopNavbarButton,
        HomeNavbarButton,
        DeckNavbarButton,
        SocialNavbarButton
    }

    public class NavbarBehaviour : MonoBehaviour
    {
        private VisualElement rootElement;
        private string buttonsContainerName = "NavbarButtonsContainer";
        private string genericButtonName = "NavButton";
        private Action<NavbarButton> Handler;

        private Dictionary<NavbarButton, NavbarButtonController> navControllers = new Dictionary<NavbarButton, NavbarButtonController>();

        public void OnEnable()
        {
            rootElement = GetComponent<UIDocument>().rootVisualElement;

            Setup(rootElement, (button) =>
            {

            });
        }

        private void Setup(VisualElement navbarElement, Action<NavbarButton> Handler)
        {
            this.Handler = Handler;

            var buttonsContainerElement = navbarElement.Q(buttonsContainerName);

            foreach (VisualElement entry in buttonsContainerElement.Children())
            {
                var name = entry.name;

                Logger.Debug($"Nav instantiated {name}");

                if (!Enum.IsDefined(typeof(NavbarButton), name)) return;

                var enumName = (NavbarButton)Enum.Parse(typeof(NavbarButton), name);

                var button = entry.Q<Button>(genericButtonName);

                var controller = new NavbarButtonController(button, () =>
                {
                    DeactivateAll(enumName);
                    Handler(enumName);
                });

                navControllers.Add(enumName, controller);
            }
        }

        private void DeactivateAll(NavbarButton currentName)
        {
            foreach (KeyValuePair<NavbarButton, NavbarButtonController> entry in navControllers)
            {
                if (entry.Key != currentName) entry.Value.Deactivate();
            }
        }

        public void Activate(NavbarButton buttonName)
        {
            var controller = navControllers[buttonName];

            if (controller != null)
                controller.Execute();
        }
    }

}