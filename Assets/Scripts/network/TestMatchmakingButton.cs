using UnityEngine;
using UnityEngine.SceneManagement;
using Pandora.Deck;
using Pandora.Deck.UI;
using System.Collections.Generic;
using System.Linq;

namespace Pandora.Network {
    public class TestMatchmakingButton: MonoBehaviour {
        public bool GameSceneToLoad = false;
        List<Card> deck;

        public void Connect() {
            Logger.Debug("Connecting");

            NetworkControllerSingleton.instance.StartMatchmaking();

            NetworkControllerSingleton.instance.matchStartEvent.AddListener(LoadGameScene);

            deck = transform.parent.GetComponentInChildren<DeckSpotParentBehaviour>().Deck;

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

        public void StartMatch() {
            NetworkControllerSingleton.instance.StartMatch();
        }

        void LoadGameScene() {
            GameSceneToLoad = true;
        }

        void Update() {
            if (GameSceneToLoad) {
                SceneManager.LoadScene("GameScene");

                GameSceneToLoad = false;
            }
        }

    }

}