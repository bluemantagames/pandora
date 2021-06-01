using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Pandora.Pool;
using Pandora.UI.Menu.Event;
using System;

namespace Pandora.UI.Menu
{
    public class ViewsContainerBehaviour : MonoBehaviour
    {
        public GameObject HomeView;
        public GameObject ShopView;
        public GameObject DeckView;
        public GameObject InitialView;
        public GameObject BackgroundObject;
        public GameObject CurrentView;
        MenuEventsSingleton menuEventsSingleton;

        float viewsAnimationTime = 0.15f;
        float backgroundAnimationTime = 0.15f;
        float backgroundParallaxVelocity = 0.1f;

        public void Awake()
        {
            menuEventsSingleton = MenuEventsSingleton.instance;
        }

        public void Start()
        {
            Setup();
        }

        private void Setup()
        {
            if (InitialView == null) return;

            EnableView(InitialView, false);
        }

        public void ShowView(MenuView view)
        {
            Logger.Debug($"Show View {view}");

            switch (view)
            {
                case MenuView.HomeView:
                    EnableView(HomeView);
                    break;

                case MenuView.ShopView:
                    EnableView(ShopView);
                    break;

                case MenuView.DeckView:
                    EnableView(DeckView);
                    break;
            }

            menuEventsSingleton.EventBus.Dispatch(new ViewActive(view));
        }

        private void EnableView(GameObject view, bool animate = true)
        {
            var currentPositionX = gameObject.transform.position.x;
            var currentPositionY = gameObject.transform.position.y;
            var viewPositionX = view.transform.position.x;

            var displayPositionX = currentPositionX - viewPositionX;

            if (animate)
            {
                ActivateAll();
                MoveBackground(currentPositionX, displayPositionX);

                gameObject.transform.DOMoveX(displayPositionX, viewsAnimationTime).SetEase(Ease.InOutCubic).OnComplete(() =>
                {
                    DeactivateAllExcept(view);
                });
            }
            else
            {
                gameObject.transform.position = new Vector2(displayPositionX, currentPositionY);
                DeactivateAllExcept(view);
            }

            CurrentView = view;
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

        private void MoveBackground(float currentPositionX, float destinationX)
        {
            var currentBackgroundPositionX = BackgroundObject.transform.position.x;
            var direction = currentPositionX > destinationX ? -1 : 1;
            var amount = Math.Abs(currentPositionX - destinationX) * backgroundParallaxVelocity;
            var newPosition = direction < 0 ? currentBackgroundPositionX - amount : currentBackgroundPositionX + amount;

            BackgroundObject.transform.DOMoveX(newPosition, backgroundAnimationTime).SetEase(Ease.InOutCubic);
        }
    }
}