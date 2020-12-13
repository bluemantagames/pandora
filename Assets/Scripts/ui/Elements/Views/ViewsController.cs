using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum ViewContainer
{
    HomeContainer,
    ShopContainer
}

public class ViewsController
{

    VisualElement viewsContainer;
    Dictionary<string, VisualElement> singleViewsContainers = new Dictionary<string, VisualElement>();
    ViewContainer activeView;

    public ViewsController(VisualElement rootElement)
    {
        viewsContainer = rootElement;

        foreach (VisualElement singleViewContainer in this.viewsContainer.Children())
        {
            var name = singleViewContainer.name;

            singleViewsContainers.Add(name, singleViewContainer);
        }
    }

    private void DeactivateView(VisualElement singleViewContainer)
    {
        singleViewContainer.style.display = DisplayStyle.None;
    }

    private void ActivateView(VisualElement singleViewContainer)
    {
        singleViewContainer.style.display = DisplayStyle.Flex;
    }

    public void Show(ViewContainer view)
    {
        if (view == activeView) return;

        var activeContainer = singleViewsContainers[activeView.ToString()];
        var toShowContainer = singleViewsContainers[view.ToString()];

        if (activeContainer == null || toShowContainer == null) return;

        DeactivateView(activeContainer);
        ActivateView(toShowContainer);

        activeView = view;
    }

}