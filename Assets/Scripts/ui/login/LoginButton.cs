﻿using UnityEngine;
using UnityEngine.UI;
using Pandora.Network;
using Pandora.Network.Data;
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
        private string oldLoginButtonText = null;
        private UserSingleton userSingleton = UserSingleton.instance;

        public async UniTaskVoid ExecuteLogin(string username, string password)
        {
            var apiController = ApiControllerSingleton.instance;
            var loginResponse = await apiController.Login(username, password);

            // Setting the token and redirect if logged in
            if (loginResponse.StatusCode == HttpStatusCode.OK)
            {
                var token = loginResponse.Data.token;

                Debug.Log($"Logged in successfully with the token: {token}");

                userSingleton.Token = token;
                SceneManager.LoadScene("MainMenuScene");
            }
            else
            {
                Debug.Log($"Status code {loginResponse.StatusCode} while logging in: {loginResponse.Content}");

                // Restoring the button text
                LoginButtonText.text = oldLoginButtonText;

                if (ErrorText != null)
                {
                    var apiError = ApiControllerSingleton.DeserializeJsonResponse<ApiError>(loginResponse.Content);
                    ErrorText.text = apiError.message;
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

            oldLoginButtonText = LoginButtonText.text;
            LoginButtonText.text = "Loading...";
        }
    }
}