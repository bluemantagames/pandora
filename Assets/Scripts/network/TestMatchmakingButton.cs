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

        /// <summary>Forces a game without authentication</summary>
        public bool DevMatchmaking = false;

        public DeckSpotParentBehaviour DeckSpotParent = null;

        PlayerModelSingleton playerModelSingleton = PlayerModelSingleton.instance;

        public void Connect()
        {
            Logger.Debug("Connecting");

            if (ProdMatchmaking)
            {
                NetworkControllerSingleton.instance.isDebugBuild = false;
            }

            var deck = DevMatchmaking && DeckSpotParent != null ?
                DeckSpotParent.Deck :
                playerModelSingleton.GetActiveDeck().Select(cardName => new Card(cardName)).ToList();

            var deckStr = deck.Select(card => card.Name).ToList();

            if (DevMatchmaking)
            {
                NetworkControllerSingleton.instance.StartDevMatchmaking(deckStr);
            }
            else
            {
                NetworkControllerSingleton.instance.StartMatchmaking(deckStr);
            }

            GameObject.Find("MatchmakingButton").GetComponent<Button>().interactable = false;

            NetworkControllerSingleton.instance.matchStartEvent.AddListener(LoadGameScene);

            HandBehaviour.Deck = deck;
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