using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginLink : MonoBehaviour
{
    public void GoToLogin()
    {
        SceneManager.LoadScene("LoginScene");
    }
}
