using UnityEngine;
using UnityEngine.UI;

namespace Pandora {
    public class HealthbarBehaviour: MonoBehaviour {
        public Color OpponentColor;

        void Start() {
            var teamComponent = GetComponentInParent<TeamComponent>();

            if (teamComponent?.team != TeamComponent.assignedTeam)
                GetComponent<Image>().color = OpponentColor;
        }

    }
}