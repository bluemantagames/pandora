using UnityEngine;
using UnityEngine.UI;

namespace Pandora
{
    public class HealthbarBehaviour : MonoBehaviour
    {
        public Sprite OpponentSprite;
        Sprite originalSprite;
        Image imageComponent;

        void Start()
        {
            imageComponent = GetComponent<Image>();
            originalSprite = imageComponent.sprite;

            RefreshColor();
        }

        public void RefreshColor()
        {
            var teamComponent = GetComponentInParent<TeamComponent>();

            if (teamComponent?.team != TeamComponent.assignedTeam)
            {
                imageComponent.sprite = OpponentSprite;
            }
            else
            {
                imageComponent.sprite = originalSprite;
            }
        }

    }
}