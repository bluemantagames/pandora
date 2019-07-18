using UnityEngine;

namespace CRclone.Network {
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