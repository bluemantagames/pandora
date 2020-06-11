using UnityEngine;
using UnityEngine.UI;
using Pandora.Network;
using Cysharp.Threading.Tasks;

namespace Pandora.UI.Login
{
    public class LoginButton : MonoBehaviour
    {
        public Text LoginButtonText = null;
        public Text UsernameText = null;
        public Text PasswordText = null;
        private APIControllerSingleton apiController = APIControllerSingleton.instance;

        public async UniTaskVoid ExecuteLogin(string username, string password)
        {
            var loginResponse = await apiController.Login(username, password);
            Debug.Log($"[LOGIN] Status: ${loginResponse.StatusCode}");
        }

        public void Login() {
            if (UsernameText == null || PasswordText == null) return;
            
            var username = UsernameText.text;
            var password = PasswordText.text;

            if (username.Length == 0 || password.Length == 0) return;

            ExecuteLogin(username, password);
        }
    }
}