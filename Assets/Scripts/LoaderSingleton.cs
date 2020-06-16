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
        private ModelSingleton modelSingleton = ModelSingleton.instance;
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

            var token = modelSingleton.Token;
            var meResponse = await apiControllerSingleton.GetMe(token);

            if (meResponse.StatusCode == HttpStatusCode.OK)
            {
                modelSingleton.User = meResponse.Body.user;
                modelSingleton.DeckSlots = meResponse.Body.deckSlots;

                IsLoading = false;
                SceneManager.LoadScene("MainMenuScene");
            }
            else
            {
                IsLoading = false;
                SceneManager.LoadScene("LoginScene");
            }

        }
    }
}