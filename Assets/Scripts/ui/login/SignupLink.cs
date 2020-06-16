using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pandora.UI.Login
{
    public class SignupLink : MonoBehaviour
    {
        public void GoToSignup()
        {
            SceneManager.LoadScene("SignupScene");
        }
    }
}