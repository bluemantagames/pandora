using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Pandora.Deck;
using Pandora.Deck.UI;
using System.Collections.Generic;
using System.Linq;
using Pandora.Events;
using Pandora.UI.Menu;
using Pandora.UI.Menu.Event;
using Pandora.UI.Menu.Home;
using Pandora.Network.Data;

namespace Pandora.Network
{
    public class TestMatchmakingButton : MonoBehaviour
    {
        public bool GameSceneToLoad = false;

        /// <summary>Forces matchmaking in prod server if enabled</summary>
        public bool ProdMatchmaking = false;

        /// <summary>Forces a game without authentication</summary>
        public bool DevMatchmaking = false;
        public Text TextLoader;
        public Text TextPlay;
        public MatchmakingWarningTextBehaviour matchmakingWarningComponent;

        MenuEventsSingleton menuEventsSingleton;
        PlayerModelSingleton playerModelSingleton = PlayerModelSingleton.instance;
        string oldPlayText = null;
        bool isLoading = false;

        public void Connect()
        {
            // Clear the MatchInfo singleton for
            // a new match
            MatchInfoSingleton.Instance.ClearAll();

            var activeDeck = GetActiveDeck();
            var isDeckValid = DevMatchmaking ? true : IsDeckValid(activeDeck);

            if (!isDeckValid) return;

            Logger.Debug("Connecting");

            isLoading = true;

            // Show loader text
            SetLoading();

            AnalyticsSingleton.Instance.TrackEvent(AnalyticsSingleton.MATCHMAKING_START);

            if (ProdMatchmaking)
            {
                NetworkControllerSingleton.instance.IsDebugBuild = false;
            }

            var deck = activeDeck?.Select(cardName => new Card(cardName))?.ToList();
            var deckStr = deck?.Select(card => card.Name)?.ToList();

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

        void LoadGameScene(Opponent opponent)
        {
            GameSceneToLoad = true;
        }

        void Awake()
        {
            menuEventsSingleton = MenuEventsSingleton.instance;
            menuEventsSingleton.EventBus.Subscribe<ViewActive>(new EventSubscriber<MenuEvent>(ViewActiveHandler, "ViewActiveHandler"));

            CheckActive();
        }

        void Update()
        {
            if (GameSceneToLoad)
            {
                AnalyticsSingleton.Instance.TrackEvent(AnalyticsSingleton.MATHCMAKING_MATCH_FOUND);

                // Hide loader text
                SetPlay();

                var networkController = NetworkControllerSingleton.instance;

                if (networkController.GameSceneLoading != null)
                {
                    networkController.GameSceneLoading.allowSceneActivation = true;
                }
                else
                {
                    SceneManager.LoadScene("GameScene");
                }

                // Clear the MatchInfo singleton for
                // a new match
                MatchInfoSingleton.Instance.ClearAll();

                AnalyticsSingleton.Instance.TrackEvent(AnalyticsSingleton.MATCHMAKING_MATCH_START);

                GameSceneToLoad = false;
            }
        }

        public void CheckActive()
        {
            if (isLoading) return;

            Logger.Debug("Checking if the matchmaking button is active...");

            var activeDeck = GetActiveDeck();
            var isValid = DevMatchmaking ? true : IsDeckValid(activeDeck);

            if (!isValid)
            {
                DisableButton();
                matchmakingWarningComponent.SetWarning(MatchmakingWarning.NotEnoughCards);
            }
            else
            {
                EnableButton();
                matchmakingWarningComponent.SetWarning(null);
            }
        }

        public void WatchLive()
        {
            SceneManager.LoadScene("LiveMenuScene");
        }

        void DisableButton()
        {
            GetComponent<Button>().interactable = false;

            Color temp;

            temp = TextLoader.color;
            temp.a = 0.5f;
            TextLoader.color = temp;

            temp = TextPlay.color;
            temp.a = 0.5f;
            TextPlay.color = temp;
        }

        void EnableButton()
        {
            GetComponent<Button>().interactable = true;

            Color temp;

            temp = TextLoader.color;
            temp.a = 1f;
            TextLoader.color = temp;

            temp = TextPlay.color;
            temp.a = 1f;
            TextPlay.color = temp;
        }

        List<string> GetActiveDeck()
        {
            var activeDeck = playerModelSingleton
                .GetActiveDeck()
                ?.Where(cardName => cardName?.Count() > 0)
                ?.ToList();

            return activeDeck;
        }

        bool IsDeckValid(List<string> activeDeck)
        {
            var isValid = activeDeck != null && activeDeck.Count == Constants.DECK_CARDS_NUMBER;

            return isValid;
        }

        void ViewActiveHandler(MenuEvent ev)
        {
            CheckActive();

            if (isLoading) SetLoading();
            else SetPlay();
        }

        void SetLoading()
        {
            TextLoader.GetComponent<MatchmakingLodaderTextBehaviour>().Enable();
            oldPlayText = TextPlay.text;
            TextPlay.text = "";
        }

        void SetPlay()
        {
            TextLoader.GetComponent<MatchmakingLodaderTextBehaviour>().Disable();

            if (string.IsNullOrEmpty(TextPlay.text) && !string.IsNullOrEmpty(oldPlayText))
                TextPlay.text = oldPlayText;
        }
    }

}