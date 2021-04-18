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

        private UniTask? Show()
        {
            if (canvasComponent == null || canvasGroupComponent == null) return null;

            canvasGroupComponent.alpha = 0;
            canvasComponent.enabled = true;

            return canvasGroupComponent.DOFade(1, fadeDuration).AsyncWaitForCompletion().AsUniTask();
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
                playerModelSingleton.leaderboardPosition = meResponse.Body.leaderboardPosition;

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
            var showTask = Show();

            if (showTask != null)
                await (UniTask)showTask;

            Logger.Debug("[LoadingBehaviour] Loading user info...");

            var userInfoResult = await LoadUserInfo();

            Logger.Debug("[LoadingBehaviour] Loading the home scene...");

            await SceneManager.LoadSceneAsync("HomeScene");

            if (!LocalizationSettings.InitializationOperation.IsDone)
            {
                Logger.Debug("[LoadingBehaviour] Loading the internationalization...");
                await LocalizationSettings.InitializationOperation.Task;
            }

            if (userInfoResult)
            {
                Logger.Debug("[LoadingBehaviour] Loading complete, exiting!");
                Hide();
            }
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