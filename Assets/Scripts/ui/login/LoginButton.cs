using UnityEngine;
using UnityEngine.UI;
using Pandora.Network;
using Cysharp.Threading.Tasks;
using System.Net;
using UnityEngine.SceneManagement;

namespace Pandora.UI.Login
{
    public class LoginButton : MonoBehaviour
    {
        public Text LoginButtonText = null;
        public Text ErrorText = null;
        public InputField UsernameInput = null;
        public InputField PasswordInput = null;
        private string oldButtonText = null, usernameKey = "username", passwordKey = "password";
        private PlayerModelSingleton playerModelSingleton;
        public bool UseProdMatchmaking = false;
        bool isLoading = false;

        void Start()
        {
            playerModelSingleton = PlayerModelSingleton.instance;

            if (PlayerPrefs.HasKey(usernameKey) && PlayerPrefs.HasKey(passwordKey))
            {
                UsernameInput.text = PlayerPrefs.GetString(usernameKey);
                PasswordInput.text = PlayerPrefs.GetString(passwordKey);

                Login();
            }
        }

        public async UniTaskVoid ExecuteLogin(string username, string password)
        {
            var apiController = ApiControllerSingleton.instance;

            if (UseProdMatchmaking)
            {
                ApiControllerSingleton.instance.IsDebugBuild = false;
            }

            var loginResponse = await apiController.Login(username, password);

            // Setting the token and redirect if logged in
            if (loginResponse.StatusCode == HttpStatusCode.OK && loginResponse.Body != null)
            {
                var token = loginResponse.Body.token;

                PlayerPrefs.SetString(usernameKey, username);
                PlayerPrefs.SetString(passwordKey, password);

                Logger.Debug($"Logged in successfully with the token: {token}");

                playerModelSingleton.Token = token;
                _ = LoaderSingleton.instance.LoadMainMenu();
            }
            else
            {
                Logger.Debug($"Status code {loginResponse.StatusCode} while logging in: {loginResponse.Error.message}");

                // Restoring the button text
                LoginButtonText.text = oldButtonText;
                isLoading = false;

                if (ErrorText != null)
                {
                    ErrorText.text = loginResponse.Error.message;
                }
            }
        }

        private bool ValidateLoginForm(string username, string password)
        {
            var isValidUsername = username.Length > 0;
            var isValidPassword = password.Length > 0;

            return isValidUsername && isValidPassword;
        }

        public void Login()
        {
            if (UsernameInput == null || PasswordInput == null || isLoading) return;

            var username = UsernameInput.text;
            var password = PasswordInput.text;

            if (!ValidateLoginForm(username, password)) return;

            _ = ExecuteLogin(username, password);

            oldButtonText = LoginButtonText.text;
            LoginButtonText.text = "Loading...";
            isLoading = true;
        }
    }
}