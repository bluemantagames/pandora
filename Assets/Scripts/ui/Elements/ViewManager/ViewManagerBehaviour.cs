using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using UnityEngine.SceneManagement;

namespace Pandora.UI.Elements.ViewManager
{
    public class ViewManagerBehaviour : MonoBehaviour
    {
        UIDocument rootDocument;
        VisualElement rootElement;
        VisualElement viewManagerElement;
        string viewElementName = "ViewManager";
        private float showSortLevel;
        private float hideSortLevel = 0;
        Func<VisualElement, float> animationValueExtractor = (element) => element.style.opacity.value;
        public int AnimationVelocity = 200;

        public void OnEnable()
        {
            rootDocument = GetComponent<UIDocument>();
            rootElement = rootDocument.rootVisualElement;
            viewManagerElement = rootElement.Q(viewElementName);
            showSortLevel = rootDocument.sortingOrder;

            HideAnimation();
        }

        private void HideAnimation()
        {
            if (viewManagerElement == null) return;

            rootDocument.sortingOrder = showSortLevel;
            viewManagerElement.style.opacity = 1f;

            viewManagerElement.experimental.animation.Start(animationValueExtractor, 0f, AnimationVelocity, (el, value) =>
            {
                el.style.opacity = value;

                if (value == 0f)
                    rootDocument.sortingOrder = hideSortLevel;
            });
        }

        public void ChangeScene(string sceneName)
        {
            if (viewManagerElement == null) return;

            rootDocument.sortingOrder = showSortLevel;
            viewManagerElement.style.opacity = 0f;

            viewManagerElement.experimental.animation.Start(animationValueExtractor, 1f, AnimationVelocity, (el, value) =>
            {
                el.style.opacity = value;

                if (value == 1f)
                {
                    var sceneChangeAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

                    sceneChangeAsync.completed += (_) =>
                    {
                        HideAnimation();
                    };
                }
            });
        }
    }

}