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

        PlayerModelSingleton playerModelSingleton = PlayerModelSingleton.instance;

        public void Connect()
        {
            Logger.Debug("Connecting");

            AnalyticsSingleton.Instance.TrackEvent(AnalyticsSingleton.MATCHMAKING_START);

            if (ProdMatchmaking)
            {
                NetworkControllerSingleton.instance.IsDebugBuild = false;
            }

            var activeDeck = playerModelSingleton.GetActiveDeck();

            if (activeDeck == null) return;

            var deck = activeDeck.Select(cardName => new Card(cardName)).ToList();
            var deckStr = deck.Select(card => card.Name).ToList();

            if (DevMatchmaking)
            {
                NetworkControllerSingleton.instance.StartDevMatchmaking(deckStr);
            }
            else
            {
                NetworkControllerSingleton.instance.StartMatchmaking(deckStr);
            }

            DisableButton();

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
                AnalyticsSingleton.Instance.TrackEvent(AnalyticsSingleton.MATHCMAKING_MATCH_FOUND);

                var networkController = NetworkControllerSingleton.instance;

                if (networkController.GameSceneLoading != null)
                {
                    networkController.GameSceneLoading.allowSceneActivation = true;
                }
                else
                {
                    SceneManager.LoadScene("GameScene");
                }

                AnalyticsSingleton.Instance.TrackEvent(AnalyticsSingleton.MATCHMAKING_MATCH_START);

                GameSceneToLoad = false;
            }
        }

        public void WatchLive()
        {
            SceneManager.LoadScene("LiveMenuScene");
        }

        void DisableButton()
        {
            GetComponent<Button>().interactable = false;
        }
    }

}