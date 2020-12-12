using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private VisualElement rootElement;
    private VisualElement menuViewsContainer;
    private void OnEnable()
    {
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        menuViewsContainer = rootElement.Q("MenuViewsContainer");

        menuViewsContainer.Q("HomeContainer").style.display = DisplayStyle.None;
        menuViewsContainer.Q("ShopContainer").style.display = DisplayStyle.Flex;
    }
}
