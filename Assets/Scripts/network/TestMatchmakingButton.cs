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
        public GameObject TextLoader;
        public GameObject TextPlay;

        PlayerModelSingleton playerModelSingleton = PlayerModelSingleton.instance;
        string oldPlayText = null;

        public void Connect()
        {
            var activeDeck = playerModelSingleton
                .GetActiveDeck()
                ?.Where(cardName => cardName.Count() > 0)
                ?.ToList();

            if (activeDeck == null || activeDeck.Count != Constants.DECK_CARDS_NUMBER) return;

            Logger.Debug("Connecting");

            // Show loader text
            TextLoader.GetComponent<MatchmakingLodaderTextBehaviour>().Enable();
            oldPlayText = TextPlay.GetComponent<Text>().text;
            TextPlay.GetComponent<Text>().text = "";

            AnalyticsSingleton.Instance.TrackEvent(AnalyticsSingleton.MATCHMAKING_START);

            if (ProdMatchmaking)
            {
                NetworkControllerSingleton.instance.IsDebugBuild = false;
            }

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

        public void PlayDevMatch()
        {
            Logger.Debug("Starting a dev match...");

            SceneManager.LoadScene("GameScene");
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

                // Hide loader text
                TextLoader.GetComponent<MatchmakingLodaderTextBehaviour>().Disable();
                TextPlay.GetComponent<Text>().text = oldPlayText;

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