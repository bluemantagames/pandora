using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Pandora.Deck;
using Pandora.Deck.UI;
using System.Collections.Generic;
using System.Linq;

namespace Pandora.Network {
    public class TestMatchmakingButton: MonoBehaviour {
        public bool GameSceneToLoad = false;
        public string DefaultUsername = "Anon";
        public Text UsernameObject;
        List<Card> deck;
        string username;

        public void Connect() {
            Logger.Debug("Connecting");

            deck = transform.parent.GetComponentInChildren<DeckSpotParentBehaviour>().Deck;
            username = DefaultUsername;

            if (UsernameObject.text.Length > 0) 
            {
                username = UsernameObject.text;
            }

            NetworkControllerSingleton.instance.StartMatchmaking(
                username,
                deck.ConvertAll(card => card.Name)
            );

            NetworkControllerSingleton.instance.matchStartEvent.AddListener(LoadGameScene);

            var deckWrapper = ScriptableObject.CreateInstance<DeckWrapper>();

            deckWrapper.Cards = 
                (from card in deck
                select card.Name).ToList();

            var serializedWrapper = JsonUtility.ToJson(deckWrapper);

            Logger.Debug($"Saving {serializedWrapper}");

            PlayerPrefs.SetString("DeckWrapper", serializedWrapper);
            PlayerPrefs.Save();

            HandBehaviour.Deck = deck;
        }

        public void StartMatch(string username, List<string> deck) {
            NetworkControllerSingleton.instance.StartMatch(
                new MatchParams(username, deck)
            );
        }

        void LoadGameScene() {
            GameSceneToLoad = true;
        }

        void Update() {
            if (GameSceneToLoad) 
            {
                SceneManager.LoadScene("GameScene");

                GameSceneToLoad = false;
            }
        }

    }

}