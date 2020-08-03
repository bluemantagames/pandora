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
        private string oldButtonText = null;
        private PlayerModelSingleton playerModelSingleton;

        void Awake()
        {
            playerModelSingleton = PlayerModelSingleton.instance;
        }

        public async UniTaskVoid ExecuteSignup(string username, string email, string password)
        {
            var apiController = ApiControllerSingleton.instance;
            var loginResponse = await apiController.Signup(username, email, password);

            // Setting the token and redirect if logged in
            if (loginResponse.StatusCode == HttpStatusCode.OK && loginResponse.Body != null)
            {
                var token = loginResponse.Body.token;

                Debug.Log($"Signup successful with the token: {token}");

                playerModelSingleton.Token = token;
                _ = LoaderSingleton.instance.LoadMainMenu();
            }
            else
            {
                Debug.Log($"Status code {loginResponse.StatusCode} while signup: {loginResponse.Error}");

                // Restoring the button text
                SignupButtonText.text = oldButtonText;

                if (ErrorText != null)
                {
                    ErrorText.text = loginResponse.Error.message;
                }
            }
        }

        public void Signup()
        {
            if (UsernameInput == null || EmailInput == null || PasswordInput == null) return;

            var username = UsernameInput.text;
            var email = EmailInput.text;
            var password = PasswordInput.text;

            if (username.Length == 0 || email.Length == 0 || password.Length == 0) return;

            _ = ExecuteSignup(username, email, password);

            oldButtonText = SignupButtonText.text;
            SignupButtonText.text = "Loading...";
        }
    }
}