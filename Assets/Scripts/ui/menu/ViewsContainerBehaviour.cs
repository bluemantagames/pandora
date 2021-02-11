using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Pandora.Pool;
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

                gameObject.transform.DOMoveX(displayPositionX, 0.15f).SetEase(Ease.InOutCubic).OnComplete(() =>
                {
                    DeactivateAllExcept(view);
                });
            }
            else
            {
                gameObject.transform.position = new Vector2(displayPositionX, currentPositionY);
                DeactivateAllExcept(view);
            }
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