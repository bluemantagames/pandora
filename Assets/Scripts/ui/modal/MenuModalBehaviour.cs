using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Pandora.UI.Modal
{
    public class MenuModalBehaviour : MonoBehaviour
    {
        public float fadeDuration = 0.2f;
        public GameObject Viewport;
        private Canvas canvasComponent;
        private CanvasGroup canvasGroupComponent;
        private GameObject appendedExternalComponent = null;

        void Awake()
        {
            canvasComponent = GetComponent<Canvas>();
            canvasGroupComponent = GetComponent<CanvasGroup>();

            HideNoAnimation();
        }

        public void HideNoAnimation()
        {
            if (canvasComponent == null) return;

            canvasComponent.enabled = false;
        }

        public void Hide()
        {
            if (canvasComponent == null || canvasGroupComponent == null) return;

            canvasGroupComponent.DOFade(0, fadeDuration).OnComplete(() =>
            {
                canvasComponent.enabled = false;

                DestroyAppendedComponent();
            });
        }

        public void Show()
        {
            if (canvasComponent == null || canvasGroupComponent == null) return;

            canvasGroupComponent.alpha = 0;
            canvasComponent.enabled = true;
            var canvasFade = canvasGroupComponent.DOFade(1, fadeDuration);
        }

        public void AppendComponent(GameObject externalComponent)
        {
            externalComponent.transform.parent = Viewport.transform;
            externalComponent.transform.localPosition = new Vector2(0, 0);

            appendedExternalComponent = externalComponent;
        }

        public void DestroyAppendedComponent()
        {
            if (appendedExternalComponent != null) Destroy(appendedExternalComponent);
        }
    }
}