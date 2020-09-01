using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pandora.UI.Signup
{
    public class LoginLink : MonoBehaviour
    {
        public void GoToLogin()
        {
            SceneManager.LoadScene("LoginScene");
        }
    }
}
