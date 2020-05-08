using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pandora.Network
{
    public class LiveButton : MonoBehaviour
    {
        public bool GameSceneToLoad = false;
        public Text MatchTokenObject;

        public void Connect()
        {
            var matchToken = MatchTokenObject.text;

            if (matchToken.Length <= 0) return;

            // Here starts the live
            ReplayControllerSingleton.instance.StartLive(matchToken);

            LoadGameScene();
        }

        void LoadGameScene()
        {
            GameSceneToLoad = true;
        }

        void Update()
        {
            if (GameSceneToLoad)
            {
                // Setting the map component as `IsLive`
                MapComponent.Instance.IsLive = true;

                SceneManager.LoadScene("GameScene");

                GameSceneToLoad = false;
            }
        }
    }
}