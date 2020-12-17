using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        static NavbarBehaviour instance;
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
                switch (button)
                {
                    case NavbarButton.HomeNavbarButton:
                        ChangeScene("HomeScene");
                        break;

                    case NavbarButton.ShopNavbarButton:
                        ChangeScene("ShopScene");
                        break;
                }
            });
        }

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
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

        private void ChangeScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

}