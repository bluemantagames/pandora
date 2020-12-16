using System.Collections.Generic;
using System;
using UnityEngine.UIElements;

namespace Pandora.UI.Elements.Views
{
    public enum ViewContainer
    {
        HomeContainer,
        ShopContainer
    }

    public class ViewsController
    {

        VisualElement viewsContainer;
        Dictionary<string, VisualElement> singleViewsContainers = new Dictionary<string, VisualElement>();
        ViewContainer activeView = ViewContainer.HomeContainer;

        public ViewsController(VisualElement rootElement)
        {
            viewsContainer = rootElement;

            foreach (VisualElement singleViewContainer in this.viewsContainer.Children())
            {
                var name = singleViewContainer.name;

                singleViewsContainers.Add(name, singleViewContainer);
            }
        }

        private void DeactivateView(VisualElement singleViewContainer, Action Callback)
        {
            Func<VisualElement, float> getter = (el) => el.style.opacity.value;

            singleViewContainer.experimental.animation.Start(getter, 0f, 1000, (el, opacity) =>
            {
                if (opacity == 0f)
                {
                    singleViewContainer.style.display = DisplayStyle.None;
                    Callback();
                }
            });
        }

        private void ActivateView(VisualElement singleViewContainer)
        {
            Func<VisualElement, float> getter = (el) => el.style.opacity.value;

            singleViewContainer.style.display = DisplayStyle.Flex;
            singleViewContainer.style.opacity = 0f;
            singleViewContainer.experimental.animation.Start(getter, 1f, 1000, (el, opacity) => { });
        }

        public void Show(ViewContainer view)
        {
            if (view == activeView) return;

            var activeContainer = singleViewsContainers[activeView.ToString()];
            var toShowContainer = singleViewsContainers[view.ToString()];

            if (activeContainer == null || toShowContainer == null) return;

            DeactivateView(activeContainer, () => ActivateView(toShowContainer));

            activeView = view;
        }

    }
}