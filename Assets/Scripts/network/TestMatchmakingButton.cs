using UnityEngine;

namespace Pandora.Network {
    public class TestMatchmakingButton: MonoBehaviour {
        public void Connect() {
            Debug.Log("Connecting");

            NetworkControllerSingleton.instance.StartMatchmaking();
        }

        public void StartMatch() {
            NetworkControllerSingleton.instance.StartMatch();
        }

    }

}