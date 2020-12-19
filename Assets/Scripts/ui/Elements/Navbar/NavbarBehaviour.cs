using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pandora.UI.Elements.ViewManager;
using System.Linq;
using Pandora.UI.Menu;

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
        private string viewManagerName = "ViewManager";
        private string buttonsContainerName = "NavbarButtonsContainer";
        private string genericButtonName = "NavButton";
        private Action<NavbarButton> Handler;
        private PanelSettings targetPanel;
        public ViewsContainerBehaviour ViewsContainer;

        private Dictionary<NavbarButton, NavbarButtonController> navControllers = new Dictionary<NavbarButton, NavbarButtonController>();

        public void OnEnable()
        {
            rootElement = GetComponent<UIDocument>().rootVisualElement;
            targetPanel = GetComponent<UIDocument>().panelSettings;

            Setup(rootElement, (button) =>
            {
                ChangeView(button);
            });
        }

        private void Setup(VisualElement navbarElement, Action<NavbarButton> Handler)
        {
            this.Handler = Handler;

            var buttonsContainerElement = navbarElement.Q(buttonsContainerName);

            foreach (VisualElement entry in buttonsContainerElement.Children())
            {
                var name = entry.name;

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

        private void ChangeView(NavbarButton view)
        {
            if (ViewsContainer == null) return;

            ViewsContainer.ShowView(view);
        }
    }

}