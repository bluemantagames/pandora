using UnityEngine;

namespace Pandora.Resource.Mana {
    public class ManaBarComponent: MonoBehaviour {
        public ManaMaskComponent MaskComponent; 

        void Start() {
            MaskComponent = GetComponentInChildren<ManaMaskComponent>();
        }
    }
}