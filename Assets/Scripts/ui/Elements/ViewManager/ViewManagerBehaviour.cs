using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class ViewManagerBehaviour : MonoBehaviour
{
    UIDocument rootDocument;
    VisualElement rootElement;
    VisualElement viewManagerElement;
    string viewElementName = "ViewManager";
    public int AnimationVelocity = 300;

    public void OnEnable()
    {
        rootDocument = GetComponent<UIDocument>();
        rootElement = rootDocument.rootVisualElement;
        viewManagerElement = rootElement.Q(viewElementName);

        StartAnimation();
    }

    private void StartAnimation()
    {
        if (viewManagerElement == null) return;

        viewManagerElement.style.opacity = 1f;
        Func<VisualElement, float> extractor = (element) => element.style.opacity.value;

        viewManagerElement.experimental.animation.Start(extractor, 0f, AnimationVelocity, (el, value) =>
        {
            el.style.opacity = value;

            if (value == 0f) Hide();
        });
    }

    private void Hide()
    {
        rootDocument.sortingOrder = 0;
    }
}
