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
        private string oldButtonText = null;
        private ModelSingleton modelSingleton = ModelSingleton.instance;

        public async UniTaskVoid ExecuteLogin(string username, string password)
        {
            var apiController = ApiControllerSingleton.instance;
            var loginResponse = await apiController.Login(username, password);

            // Setting the token and redirect if logged in
            if (loginResponse.StatusCode == HttpStatusCode.OK && loginResponse.Body != null)
            {
                var token = loginResponse.Body.token;

                Debug.Log($"Logged in successfully with the token: {token}");

                modelSingleton.Token = token;
                _ = LoaderSingleton.instance.LoadMainMenu();
            }
            else
            {
                Debug.Log($"Status code {loginResponse.StatusCode} while logging in: {loginResponse.Error.message}");

                // Restoring the button text
                LoginButtonText.text = oldButtonText;

                if (ErrorText != null)
                {
                    ErrorText.text = loginResponse.Error.message;
                }
            }
        }

        public void Login()
        {
            if (UsernameInput == null || PasswordInput == null) return;

            var username = UsernameInput.text;
            var password = PasswordInput.text;

            if (username.Length == 0 || password.Length == 0) return;

            _ = ExecuteLogin(username, password);

            oldButtonText = LoginButtonText.text;
            LoginButtonText.text = "Loading...";
        }
    }
}