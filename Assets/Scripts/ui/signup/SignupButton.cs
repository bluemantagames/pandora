using UnityEngine;
using UnityEngine.UI;
using Pandora.Network;
using Cysharp.Threading.Tasks;
using System.Net;
using UnityEngine.SceneManagement;

namespace Pandora.UI.Signup
{
    public class SignupButton : MonoBehaviour
    {
        public Text SignupButtonText = null;
        public Text ErrorText = null;
        public InputField UsernameInput = null;
        public InputField EmailInput = null;
        public InputField PasswordInput = null;
        public bool UseProdServer = false;
        private LoadingBehaviour loadingBehaviour;
        private string oldButtonText = null;
        private PlayerModelSingleton playerModelSingleton;
        bool isLoading = false;

        void Awake()
        {
            playerModelSingleton = PlayerModelSingleton.instance;
            loadingBehaviour = GameObject.Find(Constants.LOADING_CANVAS_OBJECT_NAME)?.GetComponent<LoadingBehaviour>();
        }

        public async UniTaskVoid ExecuteSignup(string username, string email, string password)
        {
            var apiController = ApiControllerSingleton.instance;

            if (UseProdServer)
            {
                apiController.IsDebugBuild = false;
            }

            var loginResponse = await apiController.Signup(username, email, password);

            // Setting the token and redirect if logged in
            if (loginResponse.StatusCode == HttpStatusCode.OK && loginResponse.Body != null)
            {
                var token = loginResponse.Body.token;

                Logger.Debug($"Signup successful with the token: {token}");

                playerModelSingleton.Token = token;

                if (loadingBehaviour != null)
                    _ = loadingBehaviour.LoadMainMenu();
            }
            else
            {
                Logger.Debug($"Status code {loginResponse.StatusCode} while signup: {loginResponse.Error}");

                // Restoring the button text
                SignupButtonText.text = oldButtonText;
                isLoading = false;

                if (ErrorText != null)
                {
                    ErrorText.text = loginResponse.Error.message;
                }
            }
        }

        private bool ValidateSignupForm(string username, string email, string password)
        {
            var isValidUsername = username.Length > 0;
            var isValidEmail = email.Length > 0;
            var isValidPassword = password.Length > 0;

            return isValidUsername && isValidEmail && isValidPassword;
        }

        public void Signup()
        {
            if (UsernameInput == null || EmailInput == null || PasswordInput == null || isLoading) return;

            var username = UsernameInput.text;
            var email = EmailInput.text;
            var password = PasswordInput.text;

            if (!ValidateSignupForm(username, email, password)) return;

            _ = ExecuteSignup(username, email, password);

            oldButtonText = SignupButtonText.text;
            SignupButtonText.text = "Loading...";
            isLoading = true;
        }
    }
}