using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pandora.UI.Elements.Navbar;

namespace Pandora.UI.Menu
{
    public class ViewsContainerBehaviour : MonoBehaviour
    {
        public GameObject HomeView;
        public GameObject ShopView;

        public void ShowView(NavbarButton view)
        {
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
            var newPosition = new Vector2(displayPositionX, currentPositionY);

            Logger.Debug($"Setting view container to {newPosition}");

            gameObject.transform.position = newPosition;
        }
    }
}