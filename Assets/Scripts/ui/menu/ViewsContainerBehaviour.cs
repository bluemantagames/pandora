using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pandora.UI.Elements.Navbar;
using DG.Tweening;
using Pandora.Pool;
using UnityEngine.EventSystems;
using Pandora.UI.Menu.Event;

namespace Pandora.UI.Menu
{
    public class ViewsContainerBehaviour : MonoBehaviour
    {
        private bool initialied = false;
        public GameObject HomeView;
        public GameObject ShopView;
        public GameObject DeckView;
        public GameObject InitialView;
        MenuEventsSingleton menuEventsSingleton;

        public void Awake()
        {
            menuEventsSingleton = MenuEventsSingleton.instance;

            Setup();
        }

        private void Setup()
        {
            if (InitialView == null) return;

            var canvas = GameObject.Find("Canvas");
            var canvasRect = canvas.GetComponent<RectTransform>();
            var canvasWidth = canvasRect.rect.width;
            var canvasHeight = canvasRect.rect.height;

            var computedX = gameObject.transform.localPosition.x;
            var computedY = gameObject.transform.localPosition.y;
            var reachedActive = false;

            foreach (RectTransform child in transform)
            {
                // Set the width and height for each
                // view container
                var newSize = PoolInstances.Vector2Pool.GetObject();
                newSize.x = canvasWidth;
                newSize.y = canvasHeight;

                child.sizeDelta = newSize;

                // Calculate the initial X position of the
                // container
                if (child.gameObject == InitialView)
                    reachedActive = true;
                else if (!reachedActive)
                    computedX -= newSize.x;

                PoolInstances.Vector2Pool.ReturnObject(newSize);
            }

            var newPosition = PoolInstances.Vector2Pool.GetObject();
            newPosition.x = computedX;
            newPosition.y = computedY;

            Logger.Debug($"Setting initial position {newPosition}");

            gameObject.transform.localPosition = newPosition;

            PoolInstances.Vector2Pool.ReturnObject(newPosition);

            DeactivateAllExcept(InitialView);
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

                case NavbarButton.DeckNavbarButton:
                    EnableView(DeckView);
                    break;
            }

            menuEventsSingleton.EventBus.Dispatch(new ViewActive(view));
        }

        private void EnableView(GameObject view, bool animate = false)
        {
            var currentPositionX = gameObject.transform.position.x;
            var currentPositionY = gameObject.transform.position.y;
            var viewPositionX = view.transform.position.x;

            var displayPositionX = currentPositionX - viewPositionX;

            ActivateAll();

            gameObject.transform.DOMoveX(displayPositionX, 0.15f).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                DeactivateAllExcept(view);
            });
        }

        private void ActivateAll()
        {
            Logger.Debug("Activating all views");

            foreach (Transform view in transform)
            {
                foreach (RectTransform viewChild in view)
                {
                    viewChild.gameObject.SetActive(true);
                }
            }
        }

        private void DeactivateAllExcept(GameObject exceptView)
        {
            Logger.Debug("Deactivating all views except for one");

            foreach (Transform view in transform)
            {
                var currentGameObject = view.gameObject;
                var isActive = currentGameObject == exceptView;

                foreach (RectTransform viewChild in view)
                {
                    viewChild.gameObject.SetActive(isActive);
                }
            }
        }
    }
}