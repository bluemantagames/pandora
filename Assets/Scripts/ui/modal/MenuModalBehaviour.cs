using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Pandora.UI.Modal
{
    public class MenuModalBehaviour : MonoBehaviour
    {
        public float fadeDuration = 0.2f;
        public GameObject Viewport;
        private Canvas canvasComponent;
        private CanvasGroup canvasGroupComponent;
        private GraphicRaycaster graphicRaycaster;
        private GameObject appendedExternalComponent = null;

        void Awake()
        {
            canvasComponent = GetComponent<Canvas>();
            canvasGroupComponent = GetComponent<CanvasGroup>();
            graphicRaycaster = GetComponent<GraphicRaycaster>();

            HideNoAnimation();
        }

        private void Disable()
        {
            if (canvasComponent != null) canvasComponent.enabled = false;
            if (canvasGroupComponent != null) canvasGroupComponent.enabled = false;
            if (graphicRaycaster != null) graphicRaycaster.enabled = false;
        }

        private void Enable()
        {
            if (canvasComponent != null) canvasComponent.enabled = true;
            if (canvasGroupComponent != null) canvasGroupComponent.enabled = true;
            if (graphicRaycaster != null) graphicRaycaster.enabled = true;
        }

        public void HideNoAnimation()
        {
            if (canvasComponent == null) return;

            Disable();
        }

        public void Hide()
        {
            if (canvasComponent == null || canvasGroupComponent == null) return;

            canvasGroupComponent.DOFade(0, fadeDuration).OnComplete(() =>
            {
                Disable();
                DestroyAppendedComponent();
            });
        }

        public void Show()
        {
            if (canvasComponent == null || canvasGroupComponent == null) return;

            Enable();
            canvasGroupComponent.alpha = 0;
            var canvasFade = canvasGroupComponent.DOFade(1, fadeDuration);
        }

        public void AppendComponent(GameObject externalComponent)
        {
            externalComponent.transform.parent = Viewport.transform;
            externalComponent.transform.localPosition = new Vector2(0, 0);
            externalComponent.transform.localScale = new Vector3(1, 1, 1);

            appendedExternalComponent = externalComponent;
        }

        public void DestroyAppendedComponent()
        {
            if (appendedExternalComponent != null) Destroy(appendedExternalComponent);
        }
    }
}