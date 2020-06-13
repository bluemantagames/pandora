using UnityEngine;
using UnityEngine.SceneManagement;

public class SignupLink : MonoBehaviour
{
    public void GoToSignup()
    {
        SceneManager.LoadScene("SignupScene");
    }
}
