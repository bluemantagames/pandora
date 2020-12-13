using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private VisualElement rootElement;
    private VisualElement menuViewsContainer;
    private VisualElement navbarContainer;
    private NavbarController navbarController;
    private VisualElement viewsContainer;
    private ViewsController viewsController;
    private void OnEnable()
    {
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        menuViewsContainer = rootElement.Q("MenuViewsContainer");
        navbarContainer = rootElement.Q("Navbar");
        viewsContainer = rootElement.Q("MenuViewsContainer");

        viewsController = new ViewsController(viewsContainer);

        navbarController = new NavbarController(navbarContainer, (navbarButton) =>
        {
            switch (navbarButton)
            {
                case NavbarButton.HomeNavbarButton:
                    viewsController.Show(ViewContainer.HomeContainer);
                    break;

                case NavbarButton.ShopNavbarButton:
                    viewsController.Show(ViewContainer.ShopContainer);
                    break;
            }
        });

        navbarController.Activate(NavbarButton.ShopNavbarButton);
    }
}
