using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Pandora.Network;
using Pandora.Deck;

namespace Pandora.UI.HUD
{
    public class BackButtonBehaviour : MonoBehaviour
    {


        public void OnButtonPress()
        {
            NetworkControllerSingleton.instance.Stop();
            LocalDeck.Instance.Reset();
            EndGameSingleton.Reset();
            TeamComponent.Reset();

            SceneManager.LoadScene("HomeScene");
        }

    }

}