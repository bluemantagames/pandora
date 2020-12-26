using UnityEngine.SceneManagement;
using UnityEngine;
using System.Net;
using Cysharp.Threading.Tasks;
using Pandora.Network;

namespace Pandora
{
    public class LoaderSingleton
    {
        public bool IsLoading = false;

        private static LoaderSingleton privateInstance = null;
        private PlayerModelSingleton playerModelSingleton = PlayerModelSingleton.instance;
        private ApiControllerSingleton apiControllerSingleton = ApiControllerSingleton.instance;

        private LoaderSingleton() { }

        public static LoaderSingleton instance
        {
            get
            {
                if (privateInstance == null)
                {
                    privateInstance = new LoaderSingleton();
                }

                return privateInstance;
            }
        }

        /// <summary>
        /// Retrieve data from the APIs and load
        /// the MainMenu scene
        /// </summary>
        /// <returns>A UniTaskVoid</returns>
        public async UniTaskVoid LoadMainMenu()
        {
            IsLoading = true;
            SceneManager.LoadScene("LoadingScene");

            var token = playerModelSingleton.Token;
            var meResponse = await apiControllerSingleton.GetMe(token);

            if (meResponse.StatusCode == HttpStatusCode.OK)
            {
                playerModelSingleton.User = meResponse.Body.user;
                playerModelSingleton.DeckSlots = meResponse.Body.deckSlots;

                IsLoading = false;
                SceneManager.LoadScene("HomeScene");
            }
            else
            {
                IsLoading = false;
                SceneManager.LoadScene("LoginScene");
            }

        }
    }
}