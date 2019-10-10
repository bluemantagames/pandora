using UnityEngine;
using UnityEngine.UI;

namespace Pandora
{
    public class HealthbarBehaviour : MonoBehaviour
    {
        public Color OpponentColor;
        Color originalColor;
        Image imageComponent;

        void Start()
        {
            imageComponent = GetComponent<Image>();
            originalColor = imageComponent.color;

            RefreshColor();
        }

        public void RefreshColor()
        {
            var teamComponent = GetComponentInParent<TeamComponent>();

            if (teamComponent?.team != TeamComponent.assignedTeam)
            {
                imageComponent.color = OpponentColor;
            }
            else
            {
                imageComponent.color = originalColor;
            }
        }

    }
}