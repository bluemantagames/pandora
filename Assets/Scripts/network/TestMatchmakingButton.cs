using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Pandora.Deck;
using Pandora.Deck.UI;
using System.Collections.Generic;
using System.Linq;

namespace Pandora.Network
{
    public class TestMatchmakingButton : MonoBehaviour
    {
        public bool GameSceneToLoad = false;

        /// <summary>Forces matchmaking in prod server if enabled</summary>
        public bool ProdMatchmaking = false;

        List<Card> deck;
        string username;

        public void Connect()
        {
            Logger.Debug("Connecting");

            if (ProdMatchmaking)
            {
                NetworkControllerSingleton.instance.isDebugBuild = false;
            }

            NetworkControllerSingleton.instance.StartMatchmaking();

            GameObject.Find("MatchmakingButton").GetComponent<Button>().interactable = false;

            NetworkControllerSingleton.instance.matchStartEvent.AddListener(LoadGameScene);
        }

        public void StartMatch(string username, List<string> deck)
        {
            NetworkControllerSingleton.instance.StartMatch();
        }
        void LoadGameScene()
        {
            GameSceneToLoad = true;
        }

        void Update()
        {
            if (GameSceneToLoad)
            {
                SceneManager.LoadScene("GameScene");

                GameSceneToLoad = false;
            }
        }

        public void WatchLive()
        {
            SceneManager.LoadScene("LiveMenuScene");
        }

    }

}