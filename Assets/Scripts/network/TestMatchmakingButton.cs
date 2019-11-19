using UnityEngine;
using UnityEngine.SceneManagement;
using Pandora.Deck;
using Pandora.Deck.UI;

namespace Pandora.Network {
    public class TestMatchmakingButton: MonoBehaviour {
        public bool GameSceneToLoad = false;

        public void Connect() {
            Debug.Log("Connecting");

            NetworkControllerSingleton.instance.StartMatchmaking();

            NetworkControllerSingleton.instance.matchStartEvent.AddListener(LoadGameScene);

            var deck = transform.parent.GetComponentInChildren<MenuCardsParentBehaviour>().Deck;

            LocalDeck.Instance.Deck = deck;
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