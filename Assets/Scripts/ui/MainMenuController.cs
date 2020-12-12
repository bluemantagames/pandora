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
    private void OnEnable()
    {
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        menuViewsContainer = rootElement.Q("MenuViewsContainer");
        navbarContainer = rootElement.Q("Navbar");

        navbarController = new NavbarController(navbarContainer);

        Logger.Debug("Initialized main controller");
    }
}
