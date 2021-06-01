using UnityEngine;

namespace Pandora.UI.Menu.NameTag
{
    public class NameTagButtonBehaviour : MonoBehaviour {
        public GameObject NameTagContainer;
        NameTagContainerBehaviour nameTagBehaviour;

        void Start() {
            nameTagBehaviour = NameTagContainer.GetComponent<NameTagContainerBehaviour>();
        }

    }
}
