using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pandora.UI.Elements.Navbar;
using DG.Tweening;

namespace Pandora.UI.Menu
{
    public class ViewsContainerBehaviour : MonoBehaviour
    {
        private bool initialied = false;
        public GameObject HomeView;
        public GameObject ShopView;
        public GameObject InitialView;

        public void Awake()
        {
            SetInitialView();
        }

        private void SetInitialView()
        {
            if (InitialView == null) return;

            var computedX = 0f;
            var computedY = gameObject.transform.position.y;

            foreach (RectTransform child in transform)
            {
                if (child.gameObject == InitialView) break;
                else computedX -= child.rect.width;
            }

            var newPosition = new Vector2(computedX, computedY);
            Logger.Debug($"Setting initial position {newPosition}");
            gameObject.transform.position = newPosition;
        }

        public void ShowView(NavbarButton view)
        {
            Logger.Debug($"Show View {view}");

            switch (view)
            {
                case NavbarButton.HomeNavbarButton:
                    EnableView(HomeView);
                    break;

                case NavbarButton.ShopNavbarButton:
                    EnableView(ShopView);
                    break;
            }
        }

        private void EnableView(GameObject view, bool animate = false)
        {
            var currentPositionX = gameObject.transform.position.x;
            var currentPositionY = gameObject.transform.position.y;
            var viewPositionX = view.transform.position.x;

            var displayPositionX = currentPositionX - viewPositionX;

            gameObject.transform.DOMoveX(displayPositionX, 0.15f).SetEase(Ease.InOutCubic);
        }
    }
}