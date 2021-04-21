using UnityEngine;
using UnityEngine.SceneManagement;
using Pandora;
using Pandora.Network;
using Pandora.Deck;

namespace Pandora.UI.HUD
{
    public class BackButtonBehaviour : MonoBehaviour
    {
        private LoadingBehaviour loadingBehaviour;

        void Awake()
        {
            loadingBehaviour = GameObject.Find(Constants.LOADING_CANVAS_OBJECT_NAME)?.GetComponent<LoadingBehaviour>();
        }

        public void OnButtonPress()
        {
            loadingBehaviour?.EndGameToMainMenu();
        }

    }

}