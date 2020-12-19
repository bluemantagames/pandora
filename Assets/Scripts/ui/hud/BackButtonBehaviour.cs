using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pandora.UI.HUD
{
    public class BackButtonBehaviour : MonoBehaviour
    {
        public void OnButtonPress()
        {
            SceneManager.LoadScene("MainMenuScene");
        }

    }

}