using UnityEngine;
using Cysharp.Threading.Tasks;
using Pandora.Network;
using System.Net;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using DG.Tweening;

namespace Pandora
{
    public class LoadingBehaviour : MonoBehaviour
    {
        public float fadeDuration = 0.2f;
        private PlayerModelSingleton playerModelSingleton;
        private ApiControllerSingleton apiControllerSingleton;
        private Canvas canvasComponent;
        private CanvasGroup canvasGroupComponent;

        private void HideNoAnimation()
        {
            if (canvasComponent == null) return;

            canvasComponent.enabled = false;
        }

        private void Hide()
        {
            if (canvasComponent == null || canvasGroupComponent == null) return;

            canvasGroupComponent.DOFade(0, fadeDuration).OnComplete(() =>
            {
                canvasComponent.enabled = false;
            });
        }

        private void Show()
        {
            if (canvasComponent == null || canvasGroupComponent == null) return;

            canvasGroupComponent.alpha = 0;
            canvasComponent.enabled = true;
            var canvasFade = canvasGroupComponent.DOFade(1, fadeDuration);
        }

        private async UniTask<bool> LoadUserInfo()
        {
            var token = playerModelSingleton.Token;

            if (token == null) return false;

            var meResponse = await apiControllerSingleton.GetMe(token);

            if (meResponse.StatusCode == HttpStatusCode.OK)
            {
                playerModelSingleton.User = meResponse.Body.user;
                playerModelSingleton.DeckSlots = meResponse.Body.deckSlots;

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Load the main menu scene
        /// </summary>
        public async UniTaskVoid LoadMainMenu()
        {
            Show();

            var userInfoResult = await LoadUserInfo();
            await SceneManager.LoadSceneAsync("HomeScene");
            await LocalizationSettings.InitializationOperation.Task;

            if (userInfoResult) Hide();
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);

            apiControllerSingleton = ApiControllerSingleton.instance;
            playerModelSingleton = PlayerModelSingleton.instance;
            apiControllerSingleton = ApiControllerSingleton.instance;
            canvasComponent = GetComponent<Canvas>();
            canvasGroupComponent = GetComponent<CanvasGroup>();

            HideNoAnimation();
        }
    }
}